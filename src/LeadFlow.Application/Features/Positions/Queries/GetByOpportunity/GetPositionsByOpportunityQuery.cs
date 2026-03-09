using System;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Positions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Positions.Queries.GetByOpportunity;

public class GetPositionsByOpportunityQuery : IRequest<List<PositionResponse>>
{
    public Guid OpportunityId { get; }
    public GetPositionsByOpportunityQuery(Guid opportunityId) => OpportunityId = opportunityId;
}

public class GetPositionsByOpportunityQueryHandler
    : IRequestHandler<GetPositionsByOpportunityQuery, List<PositionResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPositionsByOpportunityQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<PositionResponse>> Handle(
        GetPositionsByOpportunityQuery request,
        CancellationToken cancellationToken)
    {
        // Verify opportunity exists and check access
        var opportunity = await _context.Opportunities
            .Include(o => o.Lead)
            .FirstOrDefaultAsync(o => o.Id == request.OpportunityId && !o.IsDeleted, cancellationToken);

        if (opportunity is null)
            throw new Exception("Opportunity not found.");

        // Authorization: Admin, Opportunity Owner, or Lead Owner can view positions
        if (!_currentUserService.IsAdmin
            && opportunity.OwnerUserId != _currentUserService.UserId
            && opportunity.Lead.UserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view positions for this opportunity.");
        }

        var positions = await _context.OpportunityPositions
            .Where(p => p.OpportunityId == request.OpportunityId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PositionResponse(
                p.Id,
                p.OpportunityId,
                p.RoleTitle,
                p.QuantityRequired,
                p.ExperienceMin,
                p.ExperienceMax,
                p.Skills,
                p.Location,
                p.EmploymentType,
                p.Status,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return positions;
    }
}
