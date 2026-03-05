using LeadFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Assignments.Queries;

public record InterviewResponse(
    Guid Id,
    Guid AssignmentId,
    string InterviewStage,
    string? InterviewerName,
    string? InterviewerEmail,
    DateTime ScheduledAt,
    DateTime? CompletedAt,
    string Status,
    string? Feedback
);

public record GetAssignmentInterviewsQuery(Guid AssignmentId) : IRequest<List<InterviewResponse>>;

public class GetAssignmentInterviewsQueryHandler : IRequestHandler<GetAssignmentInterviewsQuery, List<InterviewResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAssignmentInterviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InterviewResponse>> Handle(GetAssignmentInterviewsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AssignmentInterviews
            .Where(i => i.AssignmentId == request.AssignmentId)
            .OrderByDescending(i => i.ScheduledAt)
            .Select(i => new InterviewResponse(
                i.Id,
                i.AssignmentId,
                i.InterviewStage,
                i.InterviewerName,
                i.InterviewerEmail,
                i.ScheduledAt,
                i.CompletedAt,
                i.Status,
                i.Feedback
            ))
            .ToListAsync(cancellationToken);
    }
}
