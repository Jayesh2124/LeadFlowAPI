using MediatR;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetLeadDetailsReport;

public record GetLeadDetailsReportQuery(Guid LeadId, string DateFilter, DateTime? StartDate, DateTime? EndDate) : IRequest<List<LeadDetailsReportDto>>;

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
