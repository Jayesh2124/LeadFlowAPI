using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Opportunities.DTOs;

public record ChangeOpportunityStatusRequest(OpportunityStatus Status);
