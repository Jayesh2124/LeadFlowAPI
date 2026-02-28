using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Interfaces.Repositories;
using LeadFlow.Domain.Enums;
using MediatR;

namespace LeadFlow.Application.Features.Opportunities.Commands.Delete;

public record DeleteOpportunityCommand(Guid Id) : IRequest;

public class DeleteOpportunityCommandHandler : IRequestHandler<DeleteOpportunityCommand>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteOpportunityCommandHandler(
        IOpportunityRepository opportunityRepository,
        ICurrentUserService currentUserService)
    {
        _opportunityRepository = opportunityRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(request.Id, cancellationToken);
        if (opportunity == null)
        {
            throw new Exception("Opportunity not found.");
        }

        // Authorization Rule: Only Admin OR Owner
        if (!_currentUserService.IsAdmin && opportunity.OwnerUserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this opportunity.");
        }

        // Business Rule: Cannot delete if status = InExecution
        if (opportunity.Status == OpportunityStatus.InExecution)
        {
            throw new Exception("Cannot delete an opportunity that is in execution.");
        }

        // Soft delete
        opportunity.SoftDelete();

        await _opportunityRepository.UpdateAsync(opportunity, cancellationToken);
    }
}
