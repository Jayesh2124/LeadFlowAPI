using System;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetRecentEmailTasks;

public class GetRecentEmailTasksQuery : IRequest<Result<List<EmailTaskDto>>>
{
    public int Count { get; }
    public GetRecentEmailTasksQuery(int count = 10) => Count = count;
}

public class EmailTaskDto
{
    public Guid Id { get; }
    public Guid LeadId { get; }
    public string LeadName { get; }
    public string TemplateName { get; }
    public string Status { get; }
    public DateTime? ScheduledAt { get; }
    public DateTime? SentAt { get; }
    public int RetryCount { get; }
    public string? ErrorMessage { get; }

    public EmailTaskDto(
        Guid Id,
        Guid LeadId,
        string LeadName,
        string TemplateName,
        string Status,
        DateTime? ScheduledAt,
        DateTime? SentAt,
        int RetryCount,
        string? ErrorMessage)
    {
        this.Id = Id;
        this.LeadId = LeadId;
        this.LeadName = LeadName;
        this.TemplateName = TemplateName;
        this.Status = Status;
        this.ScheduledAt = ScheduledAt;
        this.SentAt = SentAt;
        this.RetryCount = RetryCount;
        this.ErrorMessage = ErrorMessage;
    }
}

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
