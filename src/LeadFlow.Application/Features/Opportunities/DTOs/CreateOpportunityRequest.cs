using System;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Opportunities.DTOs;

public class CreateOpportunityRequest
{
    public Guid LeadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public OpportunityType Type { get; set; }
    public OpportunityPriority Priority { get; set; }
    public decimal ExpectedValue { get; set; }
    public DateTime? ExpectedStartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? WorkMode { get; set; }
    public string? Duration { get; set; }
    public bool? NdaSigned { get; set; }
}
