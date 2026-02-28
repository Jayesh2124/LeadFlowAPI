using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Opportunities.DTOs;

public record CreateOpportunityRequest(
    Guid LeadId,
    string Title,
    string? Description,
    OpportunityType Type,
    OpportunityPriority Priority,
    decimal ExpectedValue,
    DateTime? ExpectedStartDate,
    DateTime? ExpectedEndDate,
    Guid? OwnerUserId);
