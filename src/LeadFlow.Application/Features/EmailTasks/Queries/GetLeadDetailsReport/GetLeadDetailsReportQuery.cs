using System;
using MediatR;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetLeadDetailsReport;

public class GetLeadDetailsReportQuery : IRequest<List<LeadDetailsReportDto>>
{
    public Guid LeadId { get; }
    public string DateFilter { get; }
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }

    public GetLeadDetailsReportQuery(Guid leadId, string dateFilter, DateTime? startDate, DateTime? endDate)
    {
        LeadId = leadId;
        DateFilter = dateFilter;
        StartDate = startDate;
        EndDate = endDate;
    }
}

public class LeadDetailsReportDto
{
    public Guid EmailTaskId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public DateTime? SentOrScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
