using MediatR;

namespace LeadFlow.Application.Features.EmailTasks.Queries.GetLeadReport;

public record GetLeadReportQuery(string DateFilter, DateTime? StartDate, DateTime? EndDate) : IRequest<List<LeadReportDto>>;

public class LeadReportDto
{
    public Guid LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string LeadEmail { get; set; } = string.Empty;
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int OpenedCount { get; set; }
}
