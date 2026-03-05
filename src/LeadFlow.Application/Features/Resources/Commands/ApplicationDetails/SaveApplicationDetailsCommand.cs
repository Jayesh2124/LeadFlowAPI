using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Commands.ApplicationDetails;

// ─── Save (upsert) Application Details ───────────────────────

public record SaveApplicationDetailsCommand(
    Guid ResourceId,
    SaveApplicationDetailsRequest Request) : IRequest<ApplicationDetailsResponse>;

public class SaveApplicationDetailsValidator : AbstractValidator<SaveApplicationDetailsCommand>
{
    public SaveApplicationDetailsValidator()
    {
        RuleFor(x => x.Request.CurrentCtc)
            .GreaterThanOrEqualTo(0).When(x => x.Request.CurrentCtc.HasValue);

        RuleFor(x => x.Request.ExpectedCtc)
            .GreaterThanOrEqualTo(0).When(x => x.Request.ExpectedCtc.HasValue);

        RuleFor(x => x.Request.NoticePeriodDays)
            .GreaterThanOrEqualTo(0).When(x => x.Request.NoticePeriodDays.HasValue);

        RuleFor(x => x.Request.PreferredLocation)
            .MaximumLength(200).When(x => x.Request.PreferredLocation is not null);

        RuleFor(x => x.Request.PortfolioUrl)
            .MaximumLength(500).When(x => x.Request.PortfolioUrl is not null);

        RuleFor(x => x.Request.Skills)
            .MaximumLength(2000).When(x => x.Request.Skills is not null);

        RuleFor(x => x.Request.Certifications)
            .MaximumLength(2000).When(x => x.Request.Certifications is not null);
    }
}

public class SaveApplicationDetailsCommandHandler
    : IRequestHandler<SaveApplicationDetailsCommand, ApplicationDetailsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public SaveApplicationDetailsCommandHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<ApplicationDetailsResponse> Handle(
        SaveApplicationDetailsCommand request, CancellationToken ct)
    {
        var resource = await _db.Resources
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && !r.IsDeleted, ct);

        if (resource is null) throw new Exception("Resource not found.");

        if (!_user.IsAdmin && resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        // Upsert pattern
        var existing = await _db.ResourceApplicationDetails
            .FirstOrDefaultAsync(a => a.ResourceId == request.ResourceId, ct);

        if (existing is null)
        {
            var detail = ResourceApplicationDetail.Create(
                resourceId:         request.ResourceId,
                currentCtc:         request.Request.CurrentCtc,
                expectedCtc:        request.Request.ExpectedCtc,
                noticePeriodDays:   request.Request.NoticePeriodDays,
                preferredLocation:  request.Request.PreferredLocation,
                availabilityDate:   request.Request.AvailabilityDate,
                willingToRelocate:  request.Request.WillingToRelocate,
                workModePreference: request.Request.WorkModePreference,
                skills:             request.Request.Skills,
                certifications:     request.Request.Certifications,
                portfolioUrl:       request.Request.PortfolioUrl,
                positionName:       request.Request.PositionName);

            _db.ResourceApplicationDetails.Add(detail);
            await _db.SaveChangesAsync(ct);

            return ToResponse(detail);
        }
        else
        {
            existing.Update(
                currentCtc:         request.Request.CurrentCtc,
                expectedCtc:        request.Request.ExpectedCtc,
                noticePeriodDays:   request.Request.NoticePeriodDays,
                preferredLocation:  request.Request.PreferredLocation,
                availabilityDate:   request.Request.AvailabilityDate,
                willingToRelocate:  request.Request.WillingToRelocate,
                workModePreference: request.Request.WorkModePreference,
                skills:             request.Request.Skills,
                certifications:     request.Request.Certifications,
                portfolioUrl:       request.Request.PortfolioUrl,
                positionName:       request.Request.PositionName);

            await _db.SaveChangesAsync(ct);
            return ToResponse(existing);
        }
    }

    private static ApplicationDetailsResponse ToResponse(ResourceApplicationDetail a)
        => new(a.ResourceId, a.CurrentCtc, a.ExpectedCtc, a.NoticePeriodDays,
               a.PreferredLocation, a.AvailabilityDate, a.WillingToRelocate,
               a.WorkModePreference, a.Skills, a.Certifications, a.PortfolioUrl,
               a.PositionName, a.CreatedAt, a.UpdatedAt);
}
