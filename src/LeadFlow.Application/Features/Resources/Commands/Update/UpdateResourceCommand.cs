using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Commands.Update;

public record UpdateResourceCommand(Guid Id, UpdateResourceRequest Request) : IRequest;

public class UpdateResourceValidator : AbstractValidator<UpdateResourceCommand>
{
    public UpdateResourceValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Resource Id is required.");

        RuleFor(v => v.Request.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(v => v.Request.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");

        RuleFor(v => v.Request.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(v => v.Request.Phone is not null);

        RuleFor(v => v.Request.TotalExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Total experience must be >= 0.")
            .When(v => v.Request.TotalExperience.HasValue);

        RuleFor(v => v.Request.CurrentLocation)
            .MaximumLength(200).WithMessage("Current location must not exceed 200 characters.")
            .When(v => v.Request.CurrentLocation is not null);

        RuleFor(v => v.Request.Summary)
            .MaximumLength(2000).WithMessage("Summary must not exceed 2000 characters.")
            .When(v => v.Request.Summary is not null);

        RuleFor(v => v.Request.Source)
            .MaximumLength(100).WithMessage("Source must not exceed 100 characters.")
            .When(v => v.Request.Source is not null);

        RuleFor(v => v.Request.Status)
            .IsInEnum().WithMessage("Invalid resource status.");
    }
}

public class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateResourceCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (resource is null)
            throw new Exception("Resource not found.");

        // Authorization: Admin OR Owner
        if (!_currentUserService.IsAdmin && resource.UserId != _currentUserService.UserId)
            throw new UnauthorizedAccessException("You do not have permission to update this resource.");

        // Check email uniqueness if email changed
        if (resource.Email != request.Request.Email)
        {
            var emailExists = await _context.Resources
                .AnyAsync(r => r.UserId == resource.UserId
                            && r.Email == request.Request.Email
                            && r.Id != resource.Id
                            && !r.IsDeleted, cancellationToken);

            if (emailExists)
                throw new Exception("A resource with this email already exists for this user.");
        }

        resource.Update(
            fullName:         request.Request.FullName,
            email:            request.Request.Email,
            phone:            request.Request.Phone,
            totalExperience:  request.Request.TotalExperience,
            currentLocation:  request.Request.CurrentLocation,
            summary:          request.Request.Summary,
            source:           request.Request.Source,
            status:           request.Request.Status);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
