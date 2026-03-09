using System;
using MediatR;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetLeadReport;

public class GetLeadReportQuery : IRequest<List<LeadReportDto>>
{
    public string DateFilter { get; }
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }

    public GetLeadReportQuery(string dateFilter, DateTime? startDate, DateTime? endDate)
    {
        DateFilter = dateFilter;
        StartDate = startDate;
        EndDate = endDate;
    }
}

public class LeadReportDto
{
    public Guid LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string LeadEmail { get; set; } = string.Empty;
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int OpenedCount { get; set; }
}
