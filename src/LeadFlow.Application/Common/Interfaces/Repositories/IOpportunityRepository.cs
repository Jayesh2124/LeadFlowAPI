using LeadFlow.Domain.Entities;

namespace LeadFlow.Application.Common.Interfaces.Repositories;

public interface IOpportunityRepository
{
    Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Opportunity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Opportunity>> GetListAsync(
        Guid? leadId,
        string? type,
        string? status,
        Guid? ownerUserId,
        bool? myOpportunities,
        string? searchTitle,
        Guid currentUserId,
        bool isAdmin,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(
        Guid? leadId,
        string? type,
        string? status,
        Guid? ownerUserId,
        bool? myOpportunities,
        string? searchTitle,
        Guid currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
    Task<List<Opportunity>> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default);
    Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
}
