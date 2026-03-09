using System;
using MediatR;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetUserDetailsReport;

public class GetUserDetailsReportQuery : IRequest<List<UserDetailsReportDto>>
{
    public Guid UserId { get; }
    public string DateFilter { get; }
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }

    public GetUserDetailsReportQuery(Guid userId, string dateFilter, DateTime? startDate, DateTime? endDate)
    {
        UserId = userId;
        DateFilter = dateFilter;
        StartDate = startDate;
        EndDate = endDate;
    }
}

public class UserDetailsReportDto
{
    public Guid LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string LeadEmail { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public DateTime SentOrScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Instant or Scheduled
}
