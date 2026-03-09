using System;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Resources.DTOs;

// ── Employment DTOs ───────────────────────────────────────────

public record CreateEmploymentRequest(
    string CompanyName,
    string Designation,
    EmploymentType EmploymentType,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string? Responsibilities);

public record UpdateEmploymentRequest(
    string CompanyName,
    string Designation,
    EmploymentType EmploymentType,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsCurrent,
    string? Responsibilities);

public class EmploymentResponse
{
    public Guid Id { get; }
    public Guid ResourceId { get; }
    public string CompanyName { get; }
    public string Designation { get; }
    public EmploymentType EmploymentType { get; }
    public DateOnly? StartDate { get; }
    public DateOnly? EndDate { get; }
    public bool IsCurrent { get; }
    public string? Responsibilities { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }

    public EmploymentResponse(
        Guid Id,
        Guid ResourceId,
        string CompanyName,
        string Designation,
        EmploymentType EmploymentType,
        DateOnly? StartDate,
        DateOnly? EndDate,
        bool IsCurrent,
        string? Responsibilities,
        DateTime CreatedAt,
        DateTime? UpdatedAt)
    {
        this.Id = Id;
        this.ResourceId = ResourceId;
        this.CompanyName = CompanyName;
        this.Designation = Designation;
        this.EmploymentType = EmploymentType;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.IsCurrent = IsCurrent;
        this.Responsibilities = Responsibilities;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
    }
}

// ── Application Details DTOs ──────────────────────────────────

public record SaveApplicationDetailsRequest(
    decimal? CurrentCtc,
    decimal? ExpectedCtc,
    int? NoticePeriodDays,
    string? PreferredLocation,
    DateOnly? AvailabilityDate,
    bool WillingToRelocate,
    WorkModePreference? WorkModePreference,
    string? Skills,
    string? Certifications,
    string? PortfolioUrl,
    string? PositionName);

public class ApplicationDetailsResponse
{
    public Guid ResourceId { get; }
    public decimal? CurrentCtc { get; }
    public decimal? ExpectedCtc { get; }
    public int? NoticePeriodDays { get; }
    public string? PreferredLocation { get; }
    public DateOnly? AvailabilityDate { get; }
    public bool WillingToRelocate { get; }
    public WorkModePreference? WorkModePreference { get; }
    public string? Skills { get; }
    public string? Certifications { get; }
    public string? PortfolioUrl { get; }
    public string? PositionName { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }

    public ApplicationDetailsResponse(
        Guid ResourceId,
        decimal? CurrentCtc,
        decimal? ExpectedCtc,
        int? NoticePeriodDays,
        string? PreferredLocation,
        DateOnly? AvailabilityDate,
        bool WillingToRelocate,
        WorkModePreference? WorkModePreference,
        string? Skills,
        string? Certifications,
        string? PortfolioUrl,
        string? PositionName,
        DateTime CreatedAt,
        DateTime? UpdatedAt)
    {
        this.ResourceId = ResourceId;
        this.CurrentCtc = CurrentCtc;
        this.ExpectedCtc = ExpectedCtc;
        this.NoticePeriodDays = NoticePeriodDays;
        this.PreferredLocation = PreferredLocation;
        this.AvailabilityDate = AvailabilityDate;
        this.WillingToRelocate = WillingToRelocate;
        this.WorkModePreference = WorkModePreference;
        this.Skills = Skills;
        this.Certifications = Certifications;
        this.PortfolioUrl = PortfolioUrl;
        this.PositionName = PositionName;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
    }
}

// ── Reference DTOs ────────────────────────────────────────────

public record CreateReferenceRequest(
    ReferenceType ReferenceType,
    string ContactName,
    string? ContactPhone,
    string? ContactEmail,
    string? VendorName,
    string? PortalName,
    string? Notes,
    Guid? ReferredByUserId,
    Guid? ReferredByLeadId);

public record UpdateReferenceRequest(
    ReferenceType ReferenceType,
    string ContactName,
    string? ContactPhone,
    string? ContactEmail,
    string? VendorName,
    string? PortalName,
    string? Notes,
    Guid? ReferredByUserId,
    Guid? ReferredByLeadId);

public class ReferenceResponse
{
    public Guid Id { get; }
    public Guid ResourceId { get; }
    public ReferenceType ReferenceType { get; }
    public string ContactName { get; }
    public string? ContactPhone { get; }
    public string? ContactEmail { get; }
    public string? VendorName { get; }
    public string? PortalName { get; }
    public string? Notes { get; }
    public Guid? ReferredByUserId { get; }
    public Guid? ReferredByLeadId { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }

    public ReferenceResponse(
        Guid Id,
        Guid ResourceId,
        ReferenceType ReferenceType,
        string ContactName,
        string? ContactPhone,
        string? ContactEmail,
        string? VendorName,
        string? PortalName,
        string? Notes,
        Guid? ReferredByUserId,
        Guid? ReferredByLeadId,
        DateTime CreatedAt,
        DateTime? UpdatedAt)
    {
        this.Id = Id;
        this.ResourceId = ResourceId;
        this.ReferenceType = ReferenceType;
        this.ContactName = ContactName;
        this.ContactPhone = ContactPhone;
        this.ContactEmail = ContactEmail;
        this.VendorName = VendorName;
        this.PortalName = PortalName;
        this.Notes = Notes;
        this.ReferredByUserId = ReferredByUserId;
        this.ReferredByLeadId = ReferredByLeadId;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
    }
}

// ── Document DTOs ─────────────────────────────────────────────

public class DocumentResponse
{
    public Guid Id { get; }
    public Guid ResourceId { get; }
    public ResourceDocumentType DocumentType { get; }
    public KycDocumentType? KycDocumentType { get; }
    public string FileName { get; }
    public string FileUrl { get; }
    public long FileSizeBytes { get; }
    public Guid UploadedByUserId { get; }
    public DateTime CreatedAt { get; }

    public DocumentResponse(
        Guid Id,
        Guid ResourceId,
        ResourceDocumentType DocumentType,
        KycDocumentType? KycDocumentType,
        string FileName,
        string FileUrl,
        long FileSizeBytes,
        Guid UploadedByUserId,
        DateTime CreatedAt)
    {
        this.Id = Id;
        this.ResourceId = ResourceId;
        this.DocumentType = DocumentType;
        this.KycDocumentType = KycDocumentType;
        this.FileName = FileName;
        this.FileUrl = FileUrl;
        this.FileSizeBytes = FileSizeBytes;
        this.UploadedByUserId = UploadedByUserId;
        this.CreatedAt = CreatedAt;
    }
}
