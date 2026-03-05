using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Opportunities.DTOs;

public record UpdateOpportunityRequest(
    string Title,
    string? Description,
    OpportunityType Type,
    OpportunityStatus Status,
    OpportunityPriority Priority,
    decimal ExpectedValue,
    DateTime? ExpectedStartDate,
    DateTime? ExpectedEndDate,
    Guid OwnerUserId,
    string? WorkMode = null,
    string? Duration = null,
    bool? NdaSigned = null);
