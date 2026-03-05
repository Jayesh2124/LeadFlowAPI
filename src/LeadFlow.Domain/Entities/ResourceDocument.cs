using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

/// <summary>
/// Stores metadata for a document uploaded against a resource (Resume, KYC, Payslip, etc.).
/// The binary content is NEVER stored in the database — only the Storage URL.
/// </summary>
public class ResourceDocument : BaseEntity
{
    public Guid ResourceId { get; private set; }

    public ResourceDocumentType DocumentType { get; private set; }

    /// <summary>Optional KYC sub-type (Aadhaar, PAN, Driving Licence…).</summary>
    public KycDocumentType? KycDocumentType { get; private set; }

    public string FileName { get; private set; } = default!;

    /// <summary>Absolute or CDN URL pointing to the stored file. Max 1000 chars.</summary>
    public string FileUrl { get; private set; } = default!;

    /// <summary>File size in bytes for display purposes.</summary>
    public long FileSizeBytes { get; private set; }

    public Guid UploadedByUserId { get; private set; }

    // ── Navigation ─────────────────────────────────────────
    public Resource Resource { get; private set; } = null!;

    public User UploadedByUser { get; private set; } = null!;

    protected ResourceDocument() { }

    public static ResourceDocument Create(
        Guid resourceId,
        ResourceDocumentType documentType,
        string fileName,
        string fileUrl,
        long fileSizeBytes,
        Guid uploadedByUserId,
        KycDocumentType? kycDocumentType = null)
    {
        return new ResourceDocument
        {
            ResourceId        = resourceId,
            DocumentType      = documentType,
            KycDocumentType   = kycDocumentType,
            FileName          = fileName,
            FileUrl           = fileUrl,
            FileSizeBytes     = fileSizeBytes,
            UploadedByUserId  = uploadedByUserId
        };
    }
}
