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

public record EmploymentResponse(
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
    DateTime? UpdatedAt);

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

public record ApplicationDetailsResponse(
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
    DateTime? UpdatedAt);

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

public record ReferenceResponse(
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
    DateTime? UpdatedAt);

// ── Document DTOs ─────────────────────────────────────────────

public record DocumentResponse(
    Guid Id,
    Guid ResourceId,
    ResourceDocumentType DocumentType,
    KycDocumentType? KycDocumentType,
    string FileName,
    string FileUrl,
    long FileSizeBytes,
    Guid UploadedByUserId,
    DateTime CreatedAt);
