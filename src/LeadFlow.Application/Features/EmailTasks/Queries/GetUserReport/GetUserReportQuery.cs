using MediatR;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetUserReport;

public record GetUserReportQuery(string DateFilter, DateTime? StartDate, DateTime? EndDate) : IRequest<List<UserReportDto>>;

public class UserReportDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ScheduledEmailsCount { get; set; }
    public int InstantEmailsCount { get; set; }
}
