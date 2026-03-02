using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Positions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Positions.Commands.Update;

public record UpdatePositionCommand(Guid Id, UpdatePositionRequest Request) : IRequest;

public class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Position Id is required.");

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

public class UpdatePositionCommandHandler : IRequestHandler<UpdatePositionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePositionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.OpportunityPositions
            .Include(p => p.Opportunity)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (position is null)
            throw new Exception("Position not found.");

        // Authorization: Admin OR Opportunity Owner
        if (!_currentUserService.IsAdmin && position.Opportunity.OwnerUserId != _currentUserService.UserId)
            throw new UnauthorizedAccessException("You do not have permission to update this position.");

        position.Update(
            roleTitle:        request.Request.RoleTitle,
            quantityRequired: request.Request.QuantityRequired,
            employmentType:   request.Request.EmploymentType,
            experienceMin:    request.Request.ExperienceMin,
            experienceMax:    request.Request.ExperienceMax,
            skills:           request.Request.Skills,
            location:         request.Request.Location);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
