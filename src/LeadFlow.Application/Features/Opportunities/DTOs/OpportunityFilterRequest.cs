namespace LeadFlow.Application.Features.Opportunities.DTOs;

public record OpportunityFilterRequest
{
    public Guid? LeadId { get; init; }
    public string? Type { get; init; }
    public string? Status { get; init; }
    public Guid? OwnerUserId { get; init; }
    public bool? MyOpportunities { get; init; }
    public string? SearchTitle { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
