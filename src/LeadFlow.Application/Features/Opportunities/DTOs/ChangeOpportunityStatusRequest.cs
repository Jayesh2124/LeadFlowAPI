using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Opportunities.DTOs;

public class ChangeOpportunityStatusRequest
{
    public OpportunityStatus Status { get; set; }
}
