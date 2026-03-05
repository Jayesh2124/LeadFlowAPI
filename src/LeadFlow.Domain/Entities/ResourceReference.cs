using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

/// <summary>
/// Tracks who referred a candidate / how the resource was sourced.
/// </summary>
public class ResourceReference : BaseEntity
{
    public Guid ResourceId { get; private set; }

    public ReferenceType ReferenceType { get; private set; }

    // ── Optional FK links to existing entities ─────────────
    /// <summary>Set when the referral came from an internal system user.</summary>
    public Guid? ReferredByUserId { get; private set; }

    /// <summary>Set when the referral came from a Lead/client contact.</summary>
    public Guid? ReferredByLeadId { get; private set; }

    // ── Vendor / Portal source fields ──────────────────────
    public string? VendorName { get; private set; }

    public string? PortalName { get; private set; }

    // ── Contact information ─────────────────────────────────
    public string ContactName { get; private set; } = default!;

    public string? ContactPhone { get; private set; }

    public string? ContactEmail { get; private set; }

    public string? Notes { get; private set; }

    // ── Navigation ─────────────────────────────────────────
    public Resource Resource { get; private set; } = null!;

    /// <summary>Nullable navigation — only populated when ReferredByUserId is set.</summary>
    public User? ReferredByUser { get; private set; }

    /// <summary>Nullable navigation — only populated when ReferredByLeadId is set.</summary>
    public Lead? ReferredByLead { get; private set; }

    protected ResourceReference() { }

    public static ResourceReference Create(
        Guid resourceId,
        ReferenceType referenceType,
        string contactName,
        Guid? referredByUserId = null,
        Guid? referredByLeadId = null,
        string? vendorName     = null,
        string? portalName     = null,
        string? contactPhone   = null,
        string? contactEmail   = null,
        string? notes          = null)
    {
        return new ResourceReference
        {
            ResourceId       = resourceId,
            ReferenceType    = referenceType,
            ContactName      = contactName,
            ReferredByUserId = referredByUserId,
            ReferredByLeadId = referredByLeadId,
            VendorName       = vendorName,
            PortalName       = portalName,
            ContactPhone     = contactPhone,
            ContactEmail     = contactEmail,
            Notes            = notes
        };
    }

    public void Update(
        ReferenceType referenceType,
        string contactName,
        Guid? referredByUserId,
        Guid? referredByLeadId,
        string? vendorName,
        string? portalName,
        string? contactPhone,
        string? contactEmail,
        string? notes)
    {
        ReferenceType    = referenceType;
        ContactName      = contactName;
        ReferredByUserId = referredByUserId;
        ReferredByLeadId = referredByLeadId;
        VendorName       = vendorName;
        PortalName       = portalName;
        ContactPhone     = contactPhone;
        ContactEmail     = contactEmail;
        Notes            = notes;
        Touch();
    }
}
