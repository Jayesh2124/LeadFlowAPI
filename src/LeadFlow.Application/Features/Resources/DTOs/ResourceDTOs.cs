using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Resources.DTOs;

public record CreateResourceRequest(
    string FullName,
    string Email,
    string? Phone = null,
    decimal? TotalExperience = null,
    string? CurrentLocation = null,
    string? Summary = null,
    string? Source = null,
    ResourceStatus Status = ResourceStatus.Active);

public record UpdateResourceRequest(
    string FullName,
    string Email,
    string? Phone = null,
    decimal? TotalExperience = null,
    string? CurrentLocation = null,
    string? Summary = null,
    string? Source = null,
    ResourceStatus Status = ResourceStatus.Active);

public record ResourceResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    decimal? TotalExperience,
    string? CurrentLocation,
    string? Summary,
    string? Source,
    ResourceStatus Status,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ResourceFilterRequest
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public decimal? MinExperience { get; init; }
    public decimal? MaxExperience { get; init; }
    public string? Location { get; init; }
    public bool? MyResources { get; init; }
    public bool? ExcludeSelected { get; init; }
    public Guid? ExcludePositionId { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

public record PagedResourcesResult(
    List<ResourceResponse> Items,
    int Total,
    int Page,
    int PageSize);
