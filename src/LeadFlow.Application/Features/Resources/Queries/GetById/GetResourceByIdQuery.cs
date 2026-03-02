using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Queries.GetById;

public record GetResourceByIdQuery(Guid Id) : IRequest<ResourceResponse>;

public class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, ResourceResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetResourceByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ResourceResponse> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var resource = await _context.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (resource is null)
            throw new Exception("Resource not found.");

        // Authorization: Admin OR Owner
        if (!_currentUserService.IsAdmin && resource.UserId != _currentUserService.UserId)
            throw new UnauthorizedAccessException("You do not have permission to view this resource.");

        return new ResourceResponse(
            resource.Id,
            resource.UserId,
            resource.FullName,
            resource.Email,
            resource.Phone,
            resource.TotalExperience,
            resource.CurrentLocation,
            resource.Summary,
            resource.Source,
            resource.Status,
            resource.IsDeleted,
            resource.CreatedAt,
            resource.UpdatedAt);
    }
}
