using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Interfaces.Repositories;
using LeadFlow.Application.Features.Opportunities.DTOs;
using MediatR;

namespace LeadFlow.Application.Features.Opportunities.Queries.GetById;

public record GetOpportunityByIdQuery(Guid Id) : IRequest<OpportunityResponse>;

public class GetOpportunityByIdQueryHandler : IRequestHandler<GetOpportunityByIdQuery, OpportunityResponse>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetOpportunityByIdQueryHandler(
        IOpportunityRepository opportunityRepository,
        ICurrentUserService currentUserService)
    {
        _opportunityRepository = opportunityRepository;
        _currentUserService = currentUserService;
    }

    public async Task<OpportunityResponse> Handle(GetOpportunityByIdQuery request, CancellationToken cancellationToken)
    {
        var opportunity = await _opportunityRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        
        if (opportunity == null)
        {
            throw new Exception("Opportunity not found.");
        }

        // Authorization rule: Admin sees all, Owner sees own, LeadOwner sees own
        if (!_currentUserService.IsAdmin && 
            opportunity.OwnerUserId != _currentUserService.UserId && 
            opportunity.Lead.UserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this opportunity.");
        }

        return new OpportunityResponse(
            Id: opportunity.Id,
            LeadId: opportunity.LeadId,
            LeadName: $"{opportunity.Lead.FirstName} {opportunity.Lead.LastName}",
            CreatedByUserId: opportunity.CreatedByUserId,
            CreatedByName: $"{opportunity.CreatedByUser.Name}",
            OwnerUserId: opportunity.OwnerUserId,
            OwnerName: $"{opportunity.OwnerUser.Name}",
            Title: opportunity.Title,
            Description: opportunity.Description,
            Type: opportunity.Type,
            Status: opportunity.Status,
            Priority: opportunity.Priority,
            ExpectedValue: opportunity.ExpectedValue,
            ExpectedStartDate: opportunity.ExpectedStartDate,
            ExpectedEndDate: opportunity.ExpectedEndDate,
            CreatedAt: opportunity.CreatedAt,
            UpdatedAt: opportunity.UpdatedAt,
            Documents: opportunity.Documents.Select(d => new OpportunityDocumentDto(
                Id: d.Id,
                FileName: d.FileName,
                FileUrl: d.FileUrl,
                UploadedAt: d.CreatedAt
            )).ToList(),
            WorkMode: opportunity.WorkMode,
            Duration: opportunity.Duration,
            NdaSigned: opportunity.NdaSigned
        );
    }
}
