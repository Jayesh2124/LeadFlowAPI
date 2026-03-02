using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Commands.Create;

public record CreateResourceCommand(CreateResourceRequest Request) : IRequest<Guid>;

public class CreateResourceValidator : AbstractValidator<CreateResourceCommand>
{
    public CreateResourceValidator()
    {
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

public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateResourceCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        // Check duplicate email for the same user
        var exists = await _context.Resources
            .AnyAsync(r => r.UserId == _currentUserService.UserId
                        && r.Email == request.Request.Email
                        && !r.IsDeleted, cancellationToken);

        if (exists)
            throw new Exception("A resource with this email already exists for your account.");

        var resource = Resource.Create(
            userId:           _currentUserService.UserId,
            fullName:         request.Request.FullName,
            email:            request.Request.Email,
            phone:            request.Request.Phone,
            totalExperience:  request.Request.TotalExperience,
            currentLocation:  request.Request.CurrentLocation,
            summary:          request.Request.Summary,
            source:           request.Request.Source,
            status:           request.Request.Status);

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync(cancellationToken);

        return resource.Id;
    }
}
