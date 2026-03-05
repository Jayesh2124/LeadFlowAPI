using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Queries.ApplicationDetails;

public record GetApplicationDetailsQuery(Guid ResourceId)
    : IRequest<ApplicationDetailsResponse?>;

public class GetApplicationDetailsQueryHandler
    : IRequestHandler<GetApplicationDetailsQuery, ApplicationDetailsResponse?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public GetApplicationDetailsQueryHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db   = db;
        _user = user;
    }

    public async Task<ApplicationDetailsResponse?> Handle(
        GetApplicationDetailsQuery request, CancellationToken ct)
    {
        var resource = await _db.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && !r.IsDeleted, ct);

        if (resource is null) throw new Exception("Resource not found.");

        if (!_user.IsAdmin && resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        var a = await _db.ResourceApplicationDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ResourceId == request.ResourceId, ct);

        if (a is null) return null;

        return new ApplicationDetailsResponse(
            a.ResourceId, a.CurrentCtc, a.ExpectedCtc, a.NoticePeriodDays,
            a.PreferredLocation, a.AvailabilityDate, a.WillingToRelocate,
            a.WorkModePreference, a.Skills, a.Certifications, a.PortfolioUrl,
            a.PositionName, a.CreatedAt, a.UpdatedAt);
    }
}
