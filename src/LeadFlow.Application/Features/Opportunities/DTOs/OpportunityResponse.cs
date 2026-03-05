using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Opportunities.DTOs;

public record OpportunityResponse(
    Guid Id,
    Guid LeadId,
    string LeadName,
    Guid CreatedByUserId,
    string CreatedByName,
    Guid OwnerUserId,
    string OwnerName,
    string Title,
    string? Description,
    OpportunityType Type,
    OpportunityStatus Status,
    OpportunityPriority Priority,
    decimal ExpectedValue,
    DateTime? ExpectedStartDate,
    DateTime? ExpectedEndDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<OpportunityDocumentDto> Documents,
    string? WorkMode = null,
    string? Duration = null,
    bool? NdaSigned = null);
    
public record OpportunityDocumentDto(
    Guid Id,
    string FileName,
    string FileUrl,
    DateTime UploadedAt);
