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
    DbSet<Opportunity> Opportunities { get; }
    DbSet<OpportunityDocument> OpportunityDocuments { get; }
    DbSet<OpportunityPosition> OpportunityPositions { get; }
    DbSet<Resource> Resources { get; }
    DbSet<Technology> Technologies { get; }
    DbSet<ResourceEmployment> ResourceEmployments { get; }
    DbSet<ResourceApplicationDetail> ResourceApplicationDetails { get; }
    DbSet<ResourceReference> ResourceReferences { get; }
    DbSet<ResourceDocument> ResourceDocuments { get; }

    DbSet<ResourceAssignment> ResourceAssignments { get; }
    DbSet<AssignmentInterview> AssignmentInterviews { get; }
    DbSet<AssignmentStageHistory> AssignmentStageHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
