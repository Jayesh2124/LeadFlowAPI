using System;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Assignments.Queries;

public record AssignmentResponse(
    Guid Id,
    Guid PositionId,
    Guid ResourceId,
    string ResourceName,
    decimal Experience,
    string Location,
    string[] Skills,
    AssignmentStage Stage,
    DateTime AssignedDate,
    bool IsSelectedElsewhere
);

public class GetAssignmentsByPositionQuery : IRequest<List<AssignmentResponse>>
{
    public Guid PositionId { get; }
    public GetAssignmentsByPositionQuery(Guid positionId) => PositionId = positionId;
}

public class GetAssignmentsByPositionQueryHandler : IRequestHandler<GetAssignmentsByPositionQuery, List<AssignmentResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAssignmentsByPositionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssignmentResponse>> Handle(GetAssignmentsByPositionQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _context.ResourceAssignments
            .Include(a => a.Resource)
            .ThenInclude(r => r.ApplicationDetail)
            .Where(a => a.PositionId == request.PositionId)
            .Select(a => new
            {
                a.Id,
                a.PositionId,
                a.ResourceId,
                ResourceName = a.Resource.FullName,
                Experience = a.Resource.TotalExperience ?? 0,
                Location = a.Resource.CurrentLocation ?? "",
                Skills = a.Resource.ApplicationDetail != null && a.Resource.ApplicationDetail.Skills != null 
                    ? a.Resource.ApplicationDetail.Skills : "",
                a.Stage,
                a.AssignedAt,
                IsSelectedElsewhere = _context.ResourceAssignments.Any(ra => 
                    ra.ResourceId == a.ResourceId && 
                    ra.PositionId != a.PositionId && 
                    (ra.Stage == AssignmentStage.Selected || ra.Stage == AssignmentStage.Onboarded))
            })
            .ToListAsync(cancellationToken);

        return assignments.Select(a => new AssignmentResponse(
            a.Id,
            a.PositionId,
            a.ResourceId,
            a.ResourceName,
            a.Experience,
            a.Location,
            string.IsNullOrWhiteSpace(a.Skills) ? Array.Empty<string>() : a.Skills.Split(',').Select(s => s.Trim()).ToArray(),
            a.Stage,
            a.AssignedAt,
            a.IsSelectedElsewhere
        )).ToList();
    }
}
