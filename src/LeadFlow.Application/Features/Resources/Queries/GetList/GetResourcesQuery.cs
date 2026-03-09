using System;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Queries.GetList;

public class GetResourcesQuery : IRequest<PagedResourcesResult>
{
    public ResourceFilterRequest Filter { get; }
    public GetResourcesQuery(ResourceFilterRequest filter) => Filter = filter;
}

public class GetResourcesQueryHandler : IRequestHandler<GetResourcesQuery, PagedResourcesResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetResourcesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResourcesResult> Handle(GetResourcesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        var query = _context.Resources
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        // Non-admins only see their own resources
        if (!_currentUserService.IsAdmin)
        {
            query = query.Where(r => r.UserId == _currentUserService.UserId);
        }
        else if (filter.MyResources == true)
        {
            query = query.Where(r => r.UserId == _currentUserService.UserId);
        }

        // Exclude selected resources filter
        if (filter.ExcludeSelected == true)
        {
            query = query.Where(r => !_context.ResourceAssignments.Any(ra => 
                ra.ResourceId == r.Id && 
                (ra.Stage == AssignmentStage.Selected || ra.Stage == AssignmentStage.Onboarded)));
        }

        // Exclude resources already assigned to a specific position
        if (filter.ExcludePositionId.HasValue)
        {
            query = query.Where(r => !_context.ResourceAssignments.Any(ra => 
                ra.ResourceId == r.Id && ra.PositionId == filter.ExcludePositionId.Value));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<ResourceStatus>(filter.Status, out var statusEnum))
            query = query.Where(r => r.Status == statusEnum);

        // Experience range filter
        if (filter.MinExperience.HasValue)
            query = query.Where(r => r.TotalExperience >= filter.MinExperience.Value);

        if (filter.MaxExperience.HasValue)
            query = query.Where(r => r.TotalExperience <= filter.MaxExperience.Value);

        // Location filter
        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            var loc = filter.Location.ToLower();
            query = query.Where(r => r.CurrentLocation != null && r.CurrentLocation.ToLower().Contains(loc));
        }

        // Search filter (name or email)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(r =>
                r.FullName.ToLower().Contains(s) ||
                r.Email.ToLower().Contains(s));
        }

        var page = filter.Page ?? 1;
        var pageSize = filter.PageSize ?? 20;

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ResourceResponse(
                r.Id,
                r.UserId,
                r.FullName,
                r.Email,
                r.Phone,
                r.TotalExperience,
                r.CurrentLocation,
                r.Summary,
                r.Source,
                r.Status,
                r.IsDeleted,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResourcesResult(items, total, page, pageSize);
    }
}
