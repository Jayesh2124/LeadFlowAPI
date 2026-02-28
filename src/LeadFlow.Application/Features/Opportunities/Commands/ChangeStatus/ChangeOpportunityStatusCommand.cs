using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Interfaces.Repositories;
using LeadFlow.Application.Features.Opportunities.DTOs;
using LeadFlow.Domain.Enums;
using MediatR;

namespace LeadFlow.Application.Features.Opportunities.Commands.ChangeStatus;

public record ChangeOpportunityStatusCommand(Guid Id, ChangeOpportunityStatusRequest Request) : IRequest;

public class ChangeOpportunityStatusValidator : AbstractValidator<ChangeOpportunityStatusCommand>
{
    public ChangeOpportunityStatusValidator()
    {
        RuleFor(v => v.Id).NotEmpty().WithMessage("OpportunityId is required.");
        RuleFor(v => v.Request.Status).IsInEnum().WithMessage("Invalid Opportunity Status.");
    }
}

public class ChangeOpportunityStatusCommandHandler : IRequestHandler<ChangeOpportunityStatusCommand>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ICurrentUserService _currentUserService;

    public ChangeOpportunityStatusCommandHandler(
        IOpportunityRepository opportunityRepository,
        ICurrentUserService currentUserService)
    {
        _opportunityRepository = opportunityRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ChangeOpportunityStatusCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(request.Id, cancellationToken);
        if (opportunity == null)
        {
            throw new Exception("Opportunity not found.");
        }

        // Authorization Rule: Only Admin OR Owner can change status
        if (!_currentUserService.IsAdmin && opportunity.OwnerUserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to change the status of this opportunity.");
        }

        // Business Rules:
        // - Cannot move from Closed -> other state
        if (opportunity.Status == OpportunityStatus.Closed)
        {
            throw new Exception("Cannot change status from Closed.");
        }

        // - Won/Lost are terminal unless Admin reopens
        bool isCurrentTerminal = opportunity.Status == OpportunityStatus.Won || opportunity.Status == OpportunityStatus.Lost;
        if (isCurrentTerminal && !_currentUserService.IsAdmin)
        {
            throw new Exception("Opportunity is in a terminal state (Won/Lost). Only Admins can change its status.");
        }

        opportunity.UpdateStatus(request.Request.Status);

        await _opportunityRepository.UpdateAsync(opportunity, cancellationToken);
    }
}
