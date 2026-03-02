using Hangfire;
using LeadFlow.API.Endpoints;
using LeadFlow.API.Middleware;
using LeadFlow.Application;
using LeadFlow.Domain.Entities;
using LeadFlow.Infrastructure;
using LeadFlow.Infrastructure.BackgroundJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text;
using static Dapper.SqlMapper;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Net.WebRequestMethods;


var builder = WebApplication.CreateBuilder(args);

// ── Application + Infrastructure ────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── JSON: treat enums as strings (e.g. "FullTime" not 2) ─────────
builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
});
// Also apply to Swagger schema generation
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// ── JWT Authentication ────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSection["Issuer"],
            ValidAudience            = jwtSection["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ── Swagger ──────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LeadFlow API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type        = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT token"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ─────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("Frontend", policy =>
        //policy.WithOrigins(
        //        builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
        //            ?? ["http://localhost:57401"])
        //      .AllowAnyHeader()
        //      .AllowAnyMethod());
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ── Middleware ───────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LeadFlow API v1");
        c.RoutePrefix = string.Empty;  // Swagger at root
    });
}

// ── Hangfire Dashboard (Admin only) ──────────────────────────
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()]
});

// ── Endpoints ────────────────────────────────────────────────
app.MapAuthEndpoints();
app.MapUserEndpoints();          // ← NEW
app.MapLeadEndpoints();
app.MapTemplateEndpoints();
app.MapEmailTaskEndpoints();
app.MapSmtpSettingsEndpoints();
app.MapSystemSettingsEndpoints();
app.MapBlobEndpoints();
app.MapOpportunityEndpoints(); // NEW
app.MapPositionEndpoints();    // Phase 2 – Task 2
app.MapResourceEndpoints();    // Phase 3 – Task 2
app.MapTechnologyEndpoints();
// ── Recurring Jobs ───────────────────────────────────────────
RecurringJob.AddOrUpdate<RetryJobService>(
    "retry-pending-tasks",
    "maintenance",
    s => s.ProcessPendingTasksAsync(CancellationToken.None),
    "*/5 * * * *",
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });


RecurringJob.AddOrUpdate<RetryJobService>(
    "cleanup-stale-tasks",
    "maintenance",
    s => s.CleanupStaleTasksAsync(CancellationToken.None),
    Cron.Daily(),
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });


// ── Apply EF Core Migrations on Startup ──────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<LeadFlow.Infrastructure.Persistence.AppDbContext>();
    await db.Database.MigrateAsync();

    // Seed Admin user if none exists
    await SeedAsync(db);
} 

app.Run();

// ── Seeder ───────────────────────────────────────────────────
static async Task SeedAsync(LeadFlow.Infrastructure.Persistence.AppDbContext db)
{
    if (!db.Users.Any(u => u.Role == "Admin"))
    {
        var admin = LeadFlow.Domain.Entities.User.Create(
            "Administrator",
            "admin@leadflow.io",
            BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
            "Admin");
        db.Users.Add(admin);
        await db.SaveChangesAsync();
        Console.WriteLine("✅ Admin seeded: admin@leadflow.io / Admin@12345");
    }

    if (!db.Technologies.Any())
    {
        var techs = new[] { 
            "Angular", "React", "Vue.js", "Node.js", "C#", ".NET Core", 
            "Java", "Spring Boot", "Python", "Django", "AWS", "Azure", 
            "GCP", "SQL Server", "PostgreSQL", "MongoDB", "Docker", "Kubernetes",
            "Ruby on Rails", "PHP", "Laravel", "Swift", "Kotlin", "Flutter", "React Native"
        };
        foreach(var t in techs) {
            db.Technologies.Add(LeadFlow.Domain.Entities.Technology.Create(t));
        }
        await db.SaveChangesAsync();
        Console.WriteLine("✅ Technologies seeded");
    }
}


// Migration commands

//dotnet ef migrations add AddLeadAdditionalFields -p src\LeadFlow.Infrastructure -s src\LeadFlow.API

//dotnet ef database update -p src\LeadFlow.Infrastructure -s src\LeadFlow.API



//Leads -
//    Location for Leads-- Add, City, State, County *, Zip / Pin Code
//    Technologyy-- dropdown of tech stack-- multi / Selection field-- Master table for Tech stack
//         --Admin rights only to change tech master table
//    website of lead contact - URL


//Opportunity : 
//    Contract / Remote
//    Contract / FTE
//    OnSite
//    Hybrid

//    Duration : more than 1, (3 months, 6, 9)

//        Documentation for Opporunity
//        SOW
//        MSA
//        NDA(Yes / NO)

//        Resource Document : 
//            1.KYC-- GOV Identity Document Only one document selection based on dropdown
//            2.CV
//            3.Payslip-- if Resource Have Some experience
//	    Documentation - N/A for Freshers