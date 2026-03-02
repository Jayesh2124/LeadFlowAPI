using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Positions.DTOs;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Positions.Commands.Create;

public record CreatePositionCommand(Guid OpportunityId, CreatePositionRequest Request) : IRequest<Guid>;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(v => v.OpportunityId)
            .NotEmpty().WithMessage("OpportunityId is required.");

        RuleFor(v => v.Request.RoleTitle)
            .NotEmpty().WithMessage("Role title is required.")
            .MaximumLength(200).WithMessage("Role title must not exceed 200 characters.");

        RuleFor(v => v.Request.QuantityRequired)
            .GreaterThan(0).WithMessage("Quantity required must be greater than zero.");

        RuleFor(v => v.Request.EmploymentType)
            .IsInEnum().WithMessage("Invalid employment type.");

        RuleFor(v => v.Request.ExperienceMax)
            .GreaterThanOrEqualTo(v => v.Request.ExperienceMin)
            .When(v => v.Request.ExperienceMin.HasValue && v.Request.ExperienceMax.HasValue)
            .WithMessage("ExperienceMax must be greater than or equal to ExperienceMin.");

        RuleFor(v => v.Request.Skills)
            .MaximumLength(2000).WithMessage("Skills must not exceed 2000 characters.")
            .When(v => v.Request.Skills is not null);

        RuleFor(v => v.Request.Location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters.")
            .When(v => v.Request.Location is not null);
    }
}

public class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreatePositionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        // Load opportunity to verify it exists and check authorization
        var opportunity = await _context.Opportunities
            .Include(o => o.Lead)
            .FirstOrDefaultAsync(o => o.Id == request.OpportunityId && !o.IsDeleted, cancellationToken);

        if (opportunity is null)
            throw new Exception("Opportunity not found.");

        // Authorization: Admin OR Opportunity Owner
        if (!_currentUserService.IsAdmin && opportunity.OwnerUserId != _currentUserService.UserId)
            throw new UnauthorizedAccessException("You do not have permission to add positions to this opportunity.");

        var position = OpportunityPosition.Create(
            opportunityId:    request.OpportunityId,
            roleTitle:        request.Request.RoleTitle,
            quantityRequired: request.Request.QuantityRequired,
            employmentType:   request.Request.EmploymentType,
            experienceMin:    request.Request.ExperienceMin,
            experienceMax:    request.Request.ExperienceMax,
            skills:           request.Request.Skills,
            location:         request.Request.Location);

        _context.OpportunityPositions.Add(position);
        await _context.SaveChangesAsync(cancellationToken);

        return position.Id;
    }
}
