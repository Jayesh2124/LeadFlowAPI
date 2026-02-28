using LeadFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetLeadDetailsReport;

public class GetLeadDetailsReportHandler(IApplicationDbContext db) : IRequestHandler<GetLeadDetailsReportQuery, List<LeadDetailsReportDto>>
{
    public async Task<List<LeadDetailsReportDto>> Handle(GetLeadDetailsReportQuery request, CancellationToken cancellationToken)
    {
        var query = db.EmailTasks
            .Include(t => t.Template)
            .Where(t => t.LeadId == request.LeadId);

        var now = DateTime.UtcNow;
        if (request.DateFilter == "Today")
        {
            var start = now.Date;
            query = query.Where(t => t.CreatedAt >= start);
        }
        else if (request.DateFilter == "Yesterday")
        {
            var start = now.Date.AddDays(-1);
            var end = now.Date;
            query = query.Where(t => t.CreatedAt >= start && t.CreatedAt < end);
        }
        else if (request.DateFilter == "Last week")
        {
            var start = now.Date.AddDays(-7);
            query = query.Where(t => t.CreatedAt >= start);
        }
        else if (request.DateFilter == "Custom" && request.StartDate.HasValue && request.EndDate.HasValue)
        {
            var start = request.StartDate.Value.ToUniversalTime();
            var end = request.EndDate.Value.ToUniversalTime().AddDays(1);
            query = query.Where(t => t.CreatedAt >= start && t.CreatedAt < end);
        }

        var results = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new LeadDetailsReportDto
            {
                EmailTaskId = t.Id,
                TemplateName = t.Template != null ? t.Template.Name : string.Empty,
                SentOrScheduledAt = t.SentAt ?? t.ScheduledAt,
                Status = t.Status.ToString(),
                Type = t.ScheduledAt <= t.CreatedAt.AddMinutes(1) ? "Instant" : "Scheduled",
                Subject = t.RenderedSubject,
                Body = t.RenderedBody
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}
