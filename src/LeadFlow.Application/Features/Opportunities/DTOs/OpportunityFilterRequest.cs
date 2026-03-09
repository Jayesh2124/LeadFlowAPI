namespace LeadFlow.Application.Features.Opportunities.DTOs;

public class OpportunityFilterRequest
{
    public Guid? LeadId { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public Guid? OwnerUserId { get; set; }
    public bool? MyOpportunities { get; set; }
    public string? SearchTitle { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}
