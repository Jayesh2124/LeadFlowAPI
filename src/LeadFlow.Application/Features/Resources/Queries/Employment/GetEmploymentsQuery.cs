using System;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Queries.Employment;

public class GetEmploymentsQuery : IRequest<List<EmploymentResponse>>
{
    public Guid ResourceId { get; }
    public GetEmploymentsQuery(Guid resourceId) => ResourceId = resourceId;
}

public class GetEmploymentsQueryHandler : IRequestHandler<GetEmploymentsQuery, List<EmploymentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public GetEmploymentsQueryHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db   = db;
        _user = user;
    }

    public async Task<List<EmploymentResponse>> Handle(GetEmploymentsQuery request, CancellationToken ct)
    {
        var resource = await _db.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && !r.IsDeleted, ct);

        if (resource is null) throw new Exception("Resource not found.");

        if (!_user.IsAdmin && resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        return await _db.ResourceEmployments
            .AsNoTracking()
            .Where(e => e.ResourceId == request.ResourceId)
            .OrderByDescending(e => e.StartDate)
            .Select(e => new EmploymentResponse(
                e.Id,
                e.ResourceId,
                e.CompanyName,
                e.Designation,
                e.EmploymentType,
                e.StartDate,
                e.EndDate,
                e.IsCurrent,
                e.Responsibilities,
                e.CreatedAt,
                e.UpdatedAt))
            .ToListAsync(ct);
    }
}
