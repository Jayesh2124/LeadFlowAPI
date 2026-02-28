using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Opportunities.DTOs;

public record OpportunitySummaryDto(
    Guid Id,
    string Title,
    string LeadName,
    OpportunityType Type,
    OpportunityStatus Status,
    OpportunityPriority Priority,
    decimal ExpectedValue,
    DateTime? ExpectedStartDate,
    DateTime CreatedAt,
    string OwnerName);
    
public record OpportunityListResponse(
    List<OpportunitySummaryDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
