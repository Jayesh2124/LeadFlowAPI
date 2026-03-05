using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Interfaces.Repositories;
using LeadFlow.Application.Features.Opportunities.DTOs;
using LeadFlow.Domain.Enums;
using MediatR;

namespace LeadFlow.Application.Features.Opportunities.Commands.Update;

public record UpdateOpportunityCommand(Guid Id, UpdateOpportunityRequest Request) : IRequest;

public class UpdateOpportunityValidator : AbstractValidator<UpdateOpportunityCommand>
{
    public UpdateOpportunityValidator()
    {
        RuleFor(v => v.Id).NotEmpty().WithMessage("OpportunityId is required.");
        RuleFor(v => v.Request.Title).NotEmpty().MaximumLength(200).WithMessage("Title is required and must not exceed 200 characters.");
        RuleFor(v => v.Request.Type).IsInEnum().WithMessage("Invalid Opportunity Type.");
        RuleFor(v => v.Request.Status).IsInEnum().WithMessage("Invalid Opportunity Status.");
        RuleFor(v => v.Request.Priority).IsInEnum().WithMessage("Invalid Opportunity Priority.");
        RuleFor(v => v.Request.ExpectedValue).GreaterThanOrEqualTo(0).WithMessage("Expected Value must be non-negative.");
        RuleFor(v => v.Request.OwnerUserId).NotEmpty().WithMessage("Owner UserId is required.");
        RuleFor(v => v.Request.ExpectedEndDate)
            .GreaterThan(v => v.Request.ExpectedStartDate)
            .When(v => v.Request.ExpectedStartDate.HasValue && v.Request.ExpectedEndDate.HasValue)
            .WithMessage("Expected End Date must be after Expected Start Date.");
    }
}

public class UpdateOpportunityCommandHandler : IRequestHandler<UpdateOpportunityCommand>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateOpportunityCommandHandler(
        IOpportunityRepository opportunityRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _opportunityRepository = opportunityRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(request.Id, cancellationToken);
        if (opportunity == null)
        {
            throw new Exception("Opportunity not found.");
        }

        // Authorization Rule: Only Admin OR Owner can update
        if (!_currentUserService.IsAdmin && opportunity.OwnerUserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this opportunity.");
        }

        // Business Rule: Cannot modify if status = Closed
        if (opportunity.Status == OpportunityStatus.Closed)
        {
            throw new Exception("Cannot modify an opportunity that is Closed.");
        }

        // Validate owner exists if changing
        if (opportunity.OwnerUserId != request.Request.OwnerUserId)
        {
            var owner = await _context.Users.FindAsync(new object[] { request.Request.OwnerUserId }, cancellationToken);
            if (owner == null)
            {
                throw new Exception("Owner user not found.");
            }
        }

        opportunity.Update(
            request.Request.Title,
            request.Request.Description,
            request.Request.Type,
            request.Request.Status,
            request.Request.Priority,
            request.Request.ExpectedValue,
            request.Request.ExpectedStartDate,
            request.Request.ExpectedEndDate,
            request.Request.OwnerUserId,
            request.Request.WorkMode,
            request.Request.Duration,
            request.Request.NdaSigned);

        await _opportunityRepository.UpdateAsync(opportunity, cancellationToken);
    }
}
