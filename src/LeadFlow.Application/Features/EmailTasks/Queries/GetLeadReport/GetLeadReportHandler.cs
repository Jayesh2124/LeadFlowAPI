using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetLeadReport;

public class GetLeadReportHandler(IApplicationDbContext db) : IRequestHandler<GetLeadReportQuery, List<LeadReportDto>>
{
    public async Task<List<LeadReportDto>> Handle(GetLeadReportQuery request, CancellationToken cancellationToken)
    {
        var query = db.EmailTasks.Include(t => t.Lead).AsQueryable();

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
            .GroupBy(t => new { t.LeadId, t.Lead.FirstName, t.Lead.LastName, t.Lead.Email })
            .Select(g => new LeadReportDto
            {
                LeadId = g.Key.LeadId,
                LeadName = (g.Key.FirstName + " " + g.Key.LastName).Trim(),
                LeadEmail = g.Key.Email ?? string.Empty,
                SentCount = g.Count(t => t.Status == EmailTaskStatus.Sent),
                FailedCount = g.Count(t => t.Status == EmailTaskStatus.Failed),
                OpenedCount = 0 // Open tracking is not implemented in the current domain model
            })
            .ToListAsync(cancellationToken);

        return results.OrderBy(x => x.LeadName).ToList();
    }
}
