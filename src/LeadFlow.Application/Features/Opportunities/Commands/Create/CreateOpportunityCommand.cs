using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Interfaces.Repositories;
using LeadFlow.Application.Features.Opportunities.DTOs;
using LeadFlow.Domain.Entities;
using LeadFlow.Domain.Enums;
using MediatR;

namespace LeadFlow.Application.Features.Opportunities.Commands.Create;

public class CreateOpportunityCommand : IRequest<Guid>
{
    public CreateOpportunityRequest Request { get; }
    public CreateOpportunityCommand(CreateOpportunityRequest request) => Request = request;
}

public class CreateOpportunityValidator : AbstractValidator<CreateOpportunityCommand>
{
    public CreateOpportunityValidator()
    {
        RuleFor(v => v.Request.LeadId).NotEmpty().WithMessage("LeadId is required.");
        RuleFor(v => v.Request.Title).NotEmpty().MaximumLength(200).WithMessage("Title is required and must not exceed 200 characters.");
        RuleFor(v => v.Request.Type).IsInEnum().WithMessage("Invalid Opportunity Type.");
        RuleFor(v => v.Request.Priority).IsInEnum().WithMessage("Invalid Opportunity Priority.");
        RuleFor(v => v.Request.ExpectedValue).GreaterThanOrEqualTo(0).WithMessage("Expected Value must be non-negative.");
        RuleFor(v => v.Request.ExpectedEndDate)
            .GreaterThan(v => v.Request.ExpectedStartDate)
            .When(v => v.Request.ExpectedStartDate.HasValue && v.Request.ExpectedEndDate.HasValue)
            .WithMessage("Expected End Date must be after Expected Start Date.");
    }
}

public class CreateOpportunityCommandHandler : IRequestHandler<CreateOpportunityCommand, Guid>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateOpportunityCommandHandler(
        IOpportunityRepository opportunityRepository,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _opportunityRepository = opportunityRepository;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads.FindAsync(new object[] { request.Request.LeadId }, cancellationToken);
        if (lead == null)
        {
            throw new Exception("Lead not found.");
        }

        var currentUserId = _currentUserService.UserId;
        var ownerUserId = request.Request.OwnerUserId ?? currentUserId;

        var owner = await _context.Users.FindAsync(new object[] { ownerUserId }, cancellationToken);
        if (owner == null)
        {
            throw new Exception("Owner user not found.");
        }

        var opportunity = Opportunity.Create(
            request.Request.LeadId,
            currentUserId,
            ownerUserId,
            request.Request.Title,
            request.Request.Description,
            request.Request.Type,
            request.Request.Priority,
            request.Request.ExpectedValue,
            request.Request.ExpectedStartDate,
            request.Request.ExpectedEndDate,
            request.Request.WorkMode,
            request.Request.Duration,
            request.Request.NdaSigned);

        await _opportunityRepository.AddAsync(opportunity, cancellationToken);
        
        return opportunity.Id;
    }
}
