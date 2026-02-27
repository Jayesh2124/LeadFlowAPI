using LeadFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetUserReport;

public class GetUserReportHandler(IApplicationDbContext db) : IRequestHandler<GetUserReportQuery, List<UserReportDto>>
{
    public async Task<List<UserReportDto>> Handle(GetUserReportQuery request, CancellationToken cancellationToken)
    {
        var query = db.EmailTasks.Include(t => t.User).AsQueryable();

        // Apply date filter
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
            var end = request.EndDate.Value.ToUniversalTime().AddDays(1); // Include end date
            query = query.Where(t => t.CreatedAt >= start && t.CreatedAt < end);
        }

        var results = await query
            .GroupBy(t => new { t.UserId, t.User.Name })
            .Select(g => new UserReportDto
            {
                UserId = g.Key.UserId,
                UserName = g.Key.Name,
                // If ScheduledAt is within 1 minute of CreatedAt, consider it instant
                InstantEmailsCount = g.Count(t => t.ScheduledAt <= t.CreatedAt.AddMinutes(1)),
                ScheduledEmailsCount = g.Count(t => t.ScheduledAt > t.CreatedAt.AddMinutes(1))
            })
            .ToListAsync(cancellationToken);

        return results.OrderBy(x => x.UserName).ToList();
    }
}
