using System.Text;
using Hangfire;
using LeadFlow.API.Endpoints;
using LeadFlow.API.Middleware;
using LeadFlow.Application;
using LeadFlow.Infrastructure;
using LeadFlow.Infrastructure.BackgroundJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// ── Application + Infrastructure ────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
}


// Migration commands
//dotnet ef database update -p src\LeadFlow.Infrastructure -s src\LeadFlow.API