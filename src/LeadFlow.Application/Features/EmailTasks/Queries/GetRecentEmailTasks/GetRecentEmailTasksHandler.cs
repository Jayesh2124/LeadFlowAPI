using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetRecentEmailTasks;

public record GetRecentEmailTasksQuery(int Count = 10) : IRequest<Result<List<EmailTaskDto>>>;

public record EmailTaskDto(
    Guid Id,
    Guid LeadId,
    string LeadName,
    string TemplateName,
    string Status,
    DateTime? ScheduledAt,
    DateTime? SentAt,
    int RetryCount,
    string? ErrorMessage
);

public class GetRecentEmailTasksHandler(IApplicationDbContext db)
    : IRequestHandler<GetRecentEmailTasksQuery, Result<List<EmailTaskDto>>>
{
    public async Task<Result<List<EmailTaskDto>>> Handle(GetRecentEmailTasksQuery q, CancellationToken ct)
    {
        var tasks = await db.EmailTasks
            .Include(t => t.Lead)
            .Include(t => t.Template)
            .OrderByDescending(t => t.CreatedAt)
            .Take(q.Count)
            .Select(t => new EmailTaskDto(
                t.Id,
                t.LeadId,
                t.Lead.FirstName + " " + t.Lead.LastName,
                t.Template.Name,
                t.Status.ToString(),
                t.ScheduledAt,
                t.SentAt,
                t.AttemptCount,
                null
            ))
            .ToListAsync(ct);

        return Result<List<EmailTaskDto>>.Success(tasks);
    }
}
