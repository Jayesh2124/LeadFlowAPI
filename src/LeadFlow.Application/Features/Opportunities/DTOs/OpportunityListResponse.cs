using System;
using System.Collections.Generic;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Opportunities.DTOs;

public class OpportunitySummaryDto
{
    public Guid Id { get; }
    public string Title { get; }
    public string LeadName { get; }
    public OpportunityType Type { get; }
    public OpportunityStatus Status { get; }
    public OpportunityPriority Priority { get; }
    public decimal ExpectedValue { get; }
    public DateTime? ExpectedStartDate { get; }
    public DateTime CreatedAt { get; }
    public string OwnerName { get; }

    public OpportunitySummaryDto(
        Guid Id,
        string Title,
        string LeadName,
        OpportunityType Type,
        OpportunityStatus Status,
        OpportunityPriority Priority,
        decimal ExpectedValue,
        DateTime? ExpectedStartDate,
        DateTime CreatedAt,
        string OwnerName)
    {
        this.Id = Id;
        this.Title = Title;
        this.LeadName = LeadName;
        this.Type = Type;
        this.Status = Status;
        this.Priority = Priority;
        this.ExpectedValue = ExpectedValue;
        this.ExpectedStartDate = ExpectedStartDate;
        this.CreatedAt = CreatedAt;
        this.OwnerName = OwnerName;
    }
}

public class OpportunityListResponse
{
    public List<OpportunitySummaryDto> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public OpportunityListResponse(
        List<OpportunitySummaryDto> Items,
        int TotalCount,
        int PageNumber,
        int PageSize)
    {
        this.Items = Items;
        this.TotalCount = TotalCount;
        this.PageNumber = PageNumber;
        this.PageSize = PageSize;
    }
}
