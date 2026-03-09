using System;
using MediatR;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetUserReport;

public class GetUserReportQuery : IRequest<List<UserReportDto>>
{
    public string DateFilter { get; }
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }

    public GetUserReportQuery(string dateFilter, DateTime? startDate, DateTime? endDate)
    {
        DateFilter = dateFilter;
        StartDate = startDate;
        EndDate = endDate;
    }
}

public class UserReportDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ScheduledEmailsCount { get; set; }
    public int InstantEmailsCount { get; set; }
}
