using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Interfaces.Repositories;
using LeadFlow.Application.Features.Opportunities.DTOs;
using MediatR;

namespace LeadFlow.Application.Features.Opportunities.Queries.GetByLead;

public record GetOpportunitiesByLeadQuery(Guid LeadId) : IRequest<List<OpportunitySummaryDto>>;

public class GetOpportunitiesByLeadQueryHandler : IRequestHandler<GetOpportunitiesByLeadQuery, List<OpportunitySummaryDto>>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public GetOpportunitiesByLeadQueryHandler(
        IOpportunityRepository opportunityRepository,
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _opportunityRepository = opportunityRepository;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<List<OpportunitySummaryDto>> Handle(GetOpportunitiesByLeadQuery request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads.FindAsync(new object[] { request.LeadId }, cancellationToken);
        if (lead == null)
        {
            throw new Exception("Lead not found.");
        }

        // Authorization Rule: Admin sees all, Owner sees own
        if (!_currentUserService.IsAdmin && lead.UserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view opportunities for this lead.");
        }

        var list = await _opportunityRepository.GetByLeadIdAsync(request.LeadId, cancellationToken);
        
        return list.Select(o => new OpportunitySummaryDto(
            Id: o.Id,
            Title: o.Title,
            LeadName: $"{o.Lead.FirstName} {o.Lead.LastName}",
            Type: o.Type,
            Status: o.Status,
            Priority: o.Priority,
            ExpectedValue: o.ExpectedValue,
            ExpectedStartDate: o.ExpectedStartDate,
            CreatedAt: o.CreatedAt,
            OwnerName: $"{o.OwnerUser.Name}"
        )).ToList();
    }
}
