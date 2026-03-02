using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Positions.DTOs;

public record CreatePositionRequest(
    string RoleTitle,
    int QuantityRequired,
    EmploymentType EmploymentType,
    int? ExperienceMin = null,
    int? ExperienceMax = null,
    string? Skills = null,
    string? Location = null);

public record UpdatePositionRequest(
    string RoleTitle,
    int QuantityRequired,
    EmploymentType EmploymentType,
    int? ExperienceMin = null,
    int? ExperienceMax = null,
    string? Skills = null,
    string? Location = null);

public record ChangePositionStatusRequest(PositionStatus Status);

public record PositionResponse(
    Guid Id,
    Guid OpportunityId,
    string RoleTitle,
    int QuantityRequired,
    int? ExperienceMin,
    int? ExperienceMax,
    string? Skills,
    string? Location,
    EmploymentType EmploymentType,
    PositionStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
