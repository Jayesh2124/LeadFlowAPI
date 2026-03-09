using System;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Queries.References;

public class GetReferencesQuery : IRequest<List<ReferenceResponse>>
{
    public Guid ResourceId { get; }
    public GetReferencesQuery(Guid resourceId) => ResourceId = resourceId;
}

public class GetReferencesQueryHandler : IRequestHandler<GetReferencesQuery, List<ReferenceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public GetReferencesQueryHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db   = db;
        _user = user;
    }

    public async Task<List<ReferenceResponse>> Handle(GetReferencesQuery request, CancellationToken ct)
    {
        var resource = await _db.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && !r.IsDeleted, ct);

        if (resource is null) throw new Exception("Resource not found.");
        if (!_user.IsAdmin && resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        return await _db.ResourceReferences
            .AsNoTracking()
            .Where(r => r.ResourceId == request.ResourceId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReferenceResponse(
                r.Id, r.ResourceId, r.ReferenceType,
                r.ContactName, r.ContactPhone, r.ContactEmail,
                r.VendorName, r.PortalName, r.Notes,
                r.ReferredByUserId, r.ReferredByLeadId,
                r.CreatedAt, r.UpdatedAt))
            .ToListAsync(ct);
    }
}
