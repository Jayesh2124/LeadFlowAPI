using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetLeadTimeline;

public record GetLeadTimelineQuery(Guid LeadId) : IRequest<Result<List<EmailTimelineDto>>>;

public record EmailTimelineDto(
    Guid Id,
    string TemplateName,
    string Subject,
    string Status,
    DateTime ScheduledAt,
    DateTime? SentAt,
    int AttemptCount,
    Guid? ParentTaskId,
    List<AttemptDto> Attempts);

public record AttemptDto(
    int Number, string Result,
    string? Error, DateTime AttemptedAt, long DurationMs);

public class GetLeadTimelineHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetLeadTimelineQuery, Result<List<EmailTimelineDto>>>
{
    public async Task<Result<List<EmailTimelineDto>>> Handle(GetLeadTimelineQuery q, CancellationToken ct)
    {
        var leadExists = await db.Leads
            .AnyAsync(l => l.Id == q.LeadId && l.UserId == currentUser.UserId, ct);
        if (!leadExists) return Result<List<EmailTimelineDto>>.Failure("Lead not found.");

        var tasks = await db.EmailTasks
            .Where(t => t.LeadId == q.LeadId && t.UserId == currentUser.UserId)
            .Include(t => t.Attempts.OrderBy(a => a.AttemptNumber))
            .Include(t => t.Template)
            .OrderByDescending(t => t.ScheduledAt)
            .Select(t => new EmailTimelineDto(
                t.Id, t.Template.Name, t.RenderedSubject,
                t.Status.ToString(), t.ScheduledAt, t.SentAt,
                t.AttemptCount, t.ParentTaskId,
                t.Attempts.Select(a => new AttemptDto(
                    a.AttemptNumber, a.Result.ToString(),
                    a.ErrorMessage, a.AttemptedAt, a.DurationMs)).ToList()))
            .ToListAsync(ct);

        return Result<List<EmailTimelineDto>>.Success(tasks);
    }
}
