using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Commands.References;

// ─── Add Reference ────────────────────────────────────────────

public record AddReferenceCommand(Guid ResourceId, CreateReferenceRequest Request)
    : IRequest<Guid>;

public class AddReferenceValidator : AbstractValidator<AddReferenceCommand>
{
    public AddReferenceValidator()
    {
        RuleFor(x => x.Request.ContactName)
            .NotEmpty().WithMessage("Contact name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Request.ContactEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.ContactEmail));

        RuleFor(x => x.Request.ContactPhone)
            .MaximumLength(20).When(x => x.Request.ContactPhone is not null);

        RuleFor(x => x.Request.VendorName)
            .MaximumLength(200).When(x => x.Request.VendorName is not null);

        RuleFor(x => x.Request.PortalName)
            .MaximumLength(200).When(x => x.Request.PortalName is not null);

        RuleFor(x => x.Request.Notes)
            .MaximumLength(2000).When(x => x.Request.Notes is not null);

        RuleFor(x => x.Request.ReferenceType).IsInEnum();
    }
}

public class AddReferenceCommandHandler : IRequestHandler<AddReferenceCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public AddReferenceCommandHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Guid> Handle(AddReferenceCommand request, CancellationToken ct)
    {
        var resource = await _db.Resources
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && !r.IsDeleted, ct);

        if (resource is null) throw new Exception("Resource not found.");
        if (!_user.IsAdmin && resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        var reference = ResourceReference.Create(
            resourceId:       request.ResourceId,
            referenceType:    request.Request.ReferenceType,
            contactName:      request.Request.ContactName,
            referredByUserId: request.Request.ReferredByUserId,
            referredByLeadId: request.Request.ReferredByLeadId,
            vendorName:       request.Request.VendorName,
            portalName:       request.Request.PortalName,
            contactPhone:     request.Request.ContactPhone,
            contactEmail:     request.Request.ContactEmail,
            notes:            request.Request.Notes);

        _db.ResourceReferences.Add(reference);
        await _db.SaveChangesAsync(ct);
        return reference.Id;
    }
}

// ─── Update Reference ─────────────────────────────────────────

public record UpdateReferenceCommand(Guid ReferenceId, UpdateReferenceRequest Request)
    : IRequest;

public class UpdateReferenceValidator : AbstractValidator<UpdateReferenceCommand>
{
    public UpdateReferenceValidator()
    {
        RuleFor(x => x.Request.ContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.ReferenceType).IsInEnum();
        RuleFor(x => x.Request.ContactPhone).MaximumLength(20)
            .When(x => x.Request.ContactPhone is not null);
        RuleFor(x => x.Request.ContactEmail).EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Request.ContactEmail));
        RuleFor(x => x.Request.Notes).MaximumLength(2000)
            .When(x => x.Request.Notes is not null);
    }
}

public class UpdateReferenceCommandHandler : IRequestHandler<UpdateReferenceCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public UpdateReferenceCommandHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task Handle(UpdateReferenceCommand request, CancellationToken ct)
    {
        var reference = await _db.ResourceReferences
            .Include(r => r.Resource)
            .FirstOrDefaultAsync(r => r.Id == request.ReferenceId, ct);

        if (reference is null) throw new Exception("Reference not found.");
        if (!_user.IsAdmin && reference.Resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        reference.Update(
            referenceType:    request.Request.ReferenceType,
            contactName:      request.Request.ContactName,
            referredByUserId: request.Request.ReferredByUserId,
            referredByLeadId: request.Request.ReferredByLeadId,
            vendorName:       request.Request.VendorName,
            portalName:       request.Request.PortalName,
            contactPhone:     request.Request.ContactPhone,
            contactEmail:     request.Request.ContactEmail,
            notes:            request.Request.Notes);

        await _db.SaveChangesAsync(ct);
    }
}

// ─── Delete Reference ─────────────────────────────────────────

public record DeleteReferenceCommand(Guid ReferenceId) : IRequest;

public class DeleteReferenceCommandHandler : IRequestHandler<DeleteReferenceCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public DeleteReferenceCommandHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task Handle(DeleteReferenceCommand request, CancellationToken ct)
    {
        var reference = await _db.ResourceReferences
            .Include(r => r.Resource)
            .FirstOrDefaultAsync(r => r.Id == request.ReferenceId, ct);

        if (reference is null) throw new Exception("Reference not found.");
        if (!_user.IsAdmin && reference.Resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        _db.ResourceReferences.Remove(reference);
        await _db.SaveChangesAsync(ct);
    }
}
