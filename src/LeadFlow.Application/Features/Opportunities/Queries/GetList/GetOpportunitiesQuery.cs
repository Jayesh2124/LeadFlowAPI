using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Interfaces.Repositories;
using LeadFlow.Application.Features.Opportunities.DTOs;
using MediatR;

namespace LeadFlow.Application.Features.Opportunities.Queries.GetList;

public record GetOpportunitiesQuery(OpportunityFilterRequest Filter) : IRequest<OpportunityListResponse>;

public class GetOpportunitiesQueryValidator : AbstractValidator<GetOpportunitiesQuery>
{
    public GetOpportunitiesQueryValidator()
    {
        RuleFor(v => v.Filter.PageNumber).GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");
        RuleFor(v => v.Filter.PageSize).GreaterThanOrEqualTo(1).WithMessage("PageSize must be at least 1.");
    }
}

public class GetOpportunitiesQueryHandler : IRequestHandler<GetOpportunitiesQuery, OpportunityListResponse>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetOpportunitiesQueryHandler(
        IOpportunityRepository opportunityRepository,
        ICurrentUserService currentUserService)
    {
        _opportunityRepository = opportunityRepository;
        _currentUserService = currentUserService;
    }

    public async Task<OpportunityListResponse> Handle(GetOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var f = request.Filter;
        var currentUserId = _currentUserService.UserId;
        var isAdmin = _currentUserService.IsAdmin;

        var items = await _opportunityRepository.GetListAsync(
            f.LeadId,
            f.Type,
            f.Status,
            f.OwnerUserId,
            f.MyOpportunities,
            f.SearchTitle,
            currentUserId,
            isAdmin,
            f.PageNumber,
            f.PageSize,
            cancellationToken);

        var count = await _opportunityRepository.GetCountAsync(
            f.LeadId,
            f.Type,
            f.Status,
            f.OwnerUserId,
            f.MyOpportunities,
            f.SearchTitle,
            currentUserId,
            isAdmin,
            cancellationToken);

        var dtoList = items.Select(o => new OpportunitySummaryDto(
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

        return new OpportunityListResponse(
            Items: dtoList,
            TotalCount: count,
            PageNumber: f.PageNumber,
            PageSize: f.PageSize);
    }
}
