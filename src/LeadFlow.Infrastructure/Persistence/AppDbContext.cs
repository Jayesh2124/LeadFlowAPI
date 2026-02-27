using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailTask> EmailTasks => Set<EmailTask>();
    public DbSet<EmailAttempt> EmailAttempts => Set<EmailAttempt>();
    public DbSet<EmailFollowup> EmailFollowups => Set<EmailFollowup>();
    public DbSet<UserSmtpSettings> UserSmtpSettings => Set<UserSmtpSettings>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Ensure domain events are never mapped to the database
        builder.Ignore<LeadFlow.Domain.Common.DomainEvent>();
    }


    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Auto-update UpdatedAt on BaseEntity
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Property(nameof(Domain.Common.BaseEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(ct);
    }
}
