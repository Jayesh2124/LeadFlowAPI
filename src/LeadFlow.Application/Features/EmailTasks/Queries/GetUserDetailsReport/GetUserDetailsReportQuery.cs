using MediatR;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetUserDetailsReport;

public record GetUserDetailsReportQuery(Guid UserId, string DateFilter, DateTime? StartDate, DateTime? EndDate) : IRequest<List<UserDetailsReportDto>>;

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
