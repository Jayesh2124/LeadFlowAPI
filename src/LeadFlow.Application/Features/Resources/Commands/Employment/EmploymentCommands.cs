using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Commands.Employment;

// ─── Add Employment ───────────────────────────────────────────

public record AddEmploymentCommand(Guid ResourceId, CreateEmploymentRequest Request)
    : IRequest<Guid>;

public class AddEmploymentValidator : AbstractValidator<AddEmploymentCommand>
{
    public AddEmploymentValidator()
    {
        RuleFor(x => x.Request.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Request.Designation)
            .NotEmpty().WithMessage("Designation is required.")
            .MaximumLength(200);

        RuleFor(x => x.Request.EmploymentType)
            .IsInEnum().WithMessage("Invalid employment type.");

        RuleFor(x => x.Request.Responsibilities)
            .MaximumLength(2000)
            .When(x => x.Request.Responsibilities is not null);
    }
}

public class AddEmploymentCommandHandler : IRequestHandler<AddEmploymentCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public AddEmploymentCommandHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Guid> Handle(AddEmploymentCommand request, CancellationToken ct)
    {
        var resource = await _db.Resources
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && !r.IsDeleted, ct);

        if (resource is null)
            throw new Exception("Resource not found.");

        if (!_user.IsAdmin && resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission to modify this resource.");

        var emp = ResourceEmployment.Create(
            resourceId:       request.ResourceId,
            companyName:      request.Request.CompanyName,
            designation:      request.Request.Designation,
            employmentType:   request.Request.EmploymentType,
            startDate:        request.Request.StartDate,
            endDate:          request.Request.EndDate,
            isCurrent:        request.Request.IsCurrent,
            responsibilities: request.Request.Responsibilities);

        _db.ResourceEmployments.Add(emp);
        await _db.SaveChangesAsync(ct);
        return emp.Id;
    }
}

// ─── Update Employment ────────────────────────────────────────

public record UpdateEmploymentCommand(Guid EmploymentId, UpdateEmploymentRequest Request)
    : IRequest;

public class UpdateEmploymentValidator : AbstractValidator<UpdateEmploymentCommand>
{
    public UpdateEmploymentValidator()
    {
        RuleFor(x => x.Request.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Designation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.EmploymentType).IsInEnum();
        RuleFor(x => x.Request.Responsibilities)
            .MaximumLength(2000).When(x => x.Request.Responsibilities is not null);
    }
}

public class UpdateEmploymentCommandHandler : IRequestHandler<UpdateEmploymentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public UpdateEmploymentCommandHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task Handle(UpdateEmploymentCommand request, CancellationToken ct)
    {
        var emp = await _db.ResourceEmployments
            .Include(e => e.Resource)
            .FirstOrDefaultAsync(e => e.Id == request.EmploymentId, ct);

        if (emp is null) throw new Exception("Employment record not found.");

        if (!_user.IsAdmin && emp.Resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        emp.Update(
            companyName:      request.Request.CompanyName,
            designation:      request.Request.Designation,
            employmentType:   request.Request.EmploymentType,
            startDate:        request.Request.StartDate,
            endDate:          request.Request.EndDate,
            isCurrent:        request.Request.IsCurrent,
            responsibilities: request.Request.Responsibilities);

        await _db.SaveChangesAsync(ct);
    }
}

// ─── Delete Employment ────────────────────────────────────────

public record DeleteEmploymentCommand(Guid EmploymentId) : IRequest;

public class DeleteEmploymentCommandHandler : IRequestHandler<DeleteEmploymentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public DeleteEmploymentCommandHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task Handle(DeleteEmploymentCommand request, CancellationToken ct)
    {
        var emp = await _db.ResourceEmployments
            .Include(e => e.Resource)
            .FirstOrDefaultAsync(e => e.Id == request.EmploymentId, ct);

        if (emp is null) throw new Exception("Employment record not found.");

        if (!_user.IsAdmin && emp.Resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        _db.ResourceEmployments.Remove(emp);
        await _db.SaveChangesAsync(ct);
    }
}
