using System;
using System.Collections.Generic;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Opportunities.DTOs;

public class OpportunityResponse
{
    public Guid Id { get; }
    public Guid LeadId { get; }
    public string LeadName { get; }
    public Guid CreatedByUserId { get; }
    public string CreatedByName { get; }
    public Guid OwnerUserId { get; }
    public string OwnerName { get; }
    public string Title { get; }
    public string? Description { get; }
    public OpportunityType Type { get; }
    public OpportunityStatus Status { get; }
    public OpportunityPriority Priority { get; }
    public decimal ExpectedValue { get; }
    public DateTime? ExpectedStartDate { get; }
    public DateTime? ExpectedEndDate { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }
    public List<OpportunityDocumentDto> Documents { get; }
    public string? WorkMode { get; }
    public string? Duration { get; }
    public bool? NdaSigned { get; }

    public OpportunityResponse(
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
        bool? NdaSigned = null)
    {
        this.Id = Id;
        this.LeadId = LeadId;
        this.LeadName = LeadName;
        this.CreatedByUserId = CreatedByUserId;
        this.CreatedByName = CreatedByName;
        this.OwnerUserId = OwnerUserId;
        this.OwnerName = OwnerName;
        this.Title = Title;
        this.Description = Description;
        this.Type = Type;
        this.Status = Status;
        this.Priority = Priority;
        this.ExpectedValue = ExpectedValue;
        this.ExpectedStartDate = ExpectedStartDate;
        this.ExpectedEndDate = ExpectedEndDate;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
        this.Documents = Documents;
        this.WorkMode = WorkMode;
        this.Duration = Duration;
        this.NdaSigned = NdaSigned;
    }
}

public class OpportunityDocumentDto
{
    public Guid Id { get; }
    public string FileName { get; }
    public string FileUrl { get; }
    public DateTime UploadedAt { get; }

    public OpportunityDocumentDto(
        Guid Id,
        string FileName,
        string FileUrl,
        DateTime UploadedAt)
    {
        this.Id = Id;
        this.FileName = FileName;
        this.FileUrl = FileUrl;
        this.UploadedAt = UploadedAt;
    }
}
