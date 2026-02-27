using LeadFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetUserDetailsReport;

public class GetUserDetailsReportHandler(IApplicationDbContext db) : IRequestHandler<GetUserDetailsReportQuery, List<UserDetailsReportDto>>
{
    public async Task<List<UserDetailsReportDto>> Handle(GetUserDetailsReportQuery request, CancellationToken cancellationToken)
    {
        var query = db.EmailTasks
            .Include(t => t.Lead)
            .Include(t => t.Template)
            .Where(t => t.UserId == request.UserId);

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
            .Select(t => new UserDetailsReportDto
            {
                LeadId = t.LeadId,
                LeadName = t.Lead.FullName ?? ((t.Lead.FirstName ?? "") + " " + (t.Lead.LastName ?? "")).Trim(),
                LeadEmail = t.Lead.Email ?? string.Empty,
                Company = t.Lead.Company ?? string.Empty,
                Position = t.Lead.Position ?? string.Empty,
                TemplateName = t.Template != null ? t.Template.Name : string.Empty,
                SentOrScheduledAt = t.SentAt ?? t.ScheduledAt,
                Status = t.Status.ToString(),
                Type = t.ScheduledAt <= t.CreatedAt.AddMinutes(1) ? "Instant" : "Scheduled"
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}
