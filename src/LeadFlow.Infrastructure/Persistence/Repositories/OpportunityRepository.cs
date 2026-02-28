using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Interfaces.Repositories;
using LeadFlow.Domain.Entities;
using LeadFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Infrastructure.Persistence.Repositories;

public class OpportunityRepository : IOpportunityRepository
{
    private readonly IApplicationDbContext _context;

    public OpportunityRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task<Opportunity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Opportunities
            .Include(o => o.Lead)
            .Include(o => o.CreatedByUser)
            .Include(o => o.OwnerUser)
            .Include(o => o.Documents)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    private IQueryable<Opportunity> BuildFilterQuery(
        Guid? leadId,
        string? type,
        string? status,
        Guid? ownerUserId,
        bool? myOpportunities,
        string? searchTitle,
        Guid currentUserId,
        bool isAdmin)
    {
        var query = _context.Opportunities
            .Include(o => o.Lead)
            .Include(o => o.OwnerUser)
            .Where(o => !o.IsDeleted).AsQueryable();

        // Data isolation rule
        if (!isAdmin)
        {
            query = query.Where(o => o.OwnerUserId == currentUserId || o.Lead.UserId == currentUserId);
        }

        if (leadId.HasValue)
        {
            query = query.Where(o => o.LeadId == leadId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<OpportunityType>(type, true, out var t))
        {
            query = query.Where(o => o.Type == t);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OpportunityStatus>(status, true, out var s))
        {
            query = query.Where(o => o.Status == s);
        }

        if (ownerUserId.HasValue)
        {
            query = query.Where(o => o.OwnerUserId == ownerUserId.Value);
        }

        if (myOpportunities.HasValue && myOpportunities.Value)
        {
            query = query.Where(o => o.OwnerUserId == currentUserId);
        }

        if (!string.IsNullOrWhiteSpace(searchTitle))
        {
            var searchStr = $"%{searchTitle.ToLower()}%";
            query = query.Where(o => EF.Functions.ILike(o.Title, searchStr));
        }

        return query;
    }

    public async Task<List<Opportunity>> GetListAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(leadId, type, status, ownerUserId, myOpportunities, searchTitle, currentUserId, isAdmin);

        return await query
            .OrderByDescending(o => o.UpdatedAt != null ? o.UpdatedAt : o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(
        Guid? leadId,
        string? type,
        string? status,
        Guid? ownerUserId,
        bool? myOpportunities,
        string? searchTitle,
        Guid currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(leadId, type, status, ownerUserId, myOpportunities, searchTitle, currentUserId, isAdmin);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<List<Opportunity>> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        return await _context.Opportunities
            .Include(o => o.Lead)
            .Include(o => o.OwnerUser)
            .Where(o => o.LeadId == leadId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        await _context.Opportunities.AddAsync(opportunity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        _context.Opportunities.Update(opportunity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
