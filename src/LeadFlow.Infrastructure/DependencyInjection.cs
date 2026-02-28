using Hangfire;
using Hangfire.PostgreSql;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Infrastructure.BackgroundJobs;
using LeadFlow.Infrastructure.Email;
using LeadFlow.Infrastructure.Persistence;
using LeadFlow.Infrastructure.Security;
using LeadFlow.Infrastructure.Services;
using LeadFlow.Application.Common.Interfaces.Repositories;
using LeadFlow.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeadFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // ── Database ──────────────────────────────────────────
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("Postgres")));


        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();

        // ── Hangfire ──────────────────────────────────────────
        services.AddHangfire(c =>
            c.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
             .UseSimpleAssemblyNameTypeSerializer()
             .UseRecommendedSerializerSettings()
             .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(
                 config.GetConnectionString("Postgres")!)));

        services.AddHangfireServer(o =>
        {
            o.Queues = ["default", "email-tasks", "bulk", "maintenance"];
            o.WorkerCount = 8;
        });

        // ── Security ──────────────────────────────────────────
        var encKey = config["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key is required in configuration.");
        services.AddSingleton<IEncryptionService>(new AesEncryptionService(encKey));
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // ── Application Services ──────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailSenderFactory, EmailSenderFactory>();
        services.AddScoped<ISmtpConnectionTester, SmtpConnectionTester>();
        services.AddScoped<IEmailTaskProcessor, HangfireEmailTaskProcessor>();
        services.AddScoped<RetryJobService>();
        services.AddSingleton<IBlobStorageService, LeadFlow.Infrastructure.Storage.AzureBlobStorageService>();

        return services;
    }
}
