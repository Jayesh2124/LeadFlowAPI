using LeadFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Lead> Leads { get; }
    DbSet<EmailTemplate> EmailTemplates { get; }
    DbSet<EmailTask> EmailTasks { get; }
    DbSet<EmailAttempt> EmailAttempts { get; }
    DbSet<EmailFollowup> EmailFollowups { get; }
    DbSet<UserSmtpSettings> UserSmtpSettings { get; }
    DbSet<SystemSettings> SystemSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
