using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

/// <summary>
/// 1:1 extension of <see cref="Resource"/> that holds job-application preferences.
/// PK = resource_id (shared primary key pattern).
/// </summary>
public class ResourceApplicationDetail
{
    /// <summary>FK and PK — same value as resources.id.</summary>
    public Guid ResourceId { get; private set; }

    public decimal? CurrentCtc { get; private set; }

    public decimal? ExpectedCtc { get; private set; }

    /// <summary>Notice period in calendar days (e.g. 30, 60, 90).</summary>
    public int? NoticePeriodDays { get; private set; }

    public string? PreferredLocation { get; private set; }

    public DateOnly? AvailabilityDate { get; private set; }

    public bool WillingToRelocate { get; private set; }

    public WorkModePreference? WorkModePreference { get; private set; }

    public string? Skills { get; private set; }

    public string? Certifications { get; private set; }

    public string? PortfolioUrl { get; private set; }
    public string? PositionName { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // ── Navigation ─────────────────────────────────────────
    public Resource Resource { get; private set; } = null!;

    protected ResourceApplicationDetail() { }

    public static ResourceApplicationDetail Create(
        Guid resourceId,
        decimal? currentCtc            = null,
        decimal? expectedCtc           = null,
        int? noticePeriodDays          = null,
        string? preferredLocation      = null,
        DateOnly? availabilityDate     = null,
        bool willingToRelocate         = false,
        WorkModePreference? workModePreference = null,
        string? skills                 = null,
        string? certifications         = null,
        string? portfolioUrl           = null,
        string? positionName           = null)
    {
        return new ResourceApplicationDetail
        {
            ResourceId         = resourceId,
            CurrentCtc         = currentCtc,
            ExpectedCtc        = expectedCtc,
            NoticePeriodDays   = noticePeriodDays,
            PreferredLocation  = preferredLocation,
            AvailabilityDate   = availabilityDate,
            WillingToRelocate  = willingToRelocate,
            WorkModePreference = workModePreference,
            Skills             = skills,
            Certifications     = certifications,
            PortfolioUrl       = portfolioUrl,
            PositionName       = positionName
        };
    }

    public void Update(
        decimal? currentCtc,
        decimal? expectedCtc,
        int? noticePeriodDays,
        string? preferredLocation,
        DateOnly? availabilityDate,
        bool willingToRelocate,
        WorkModePreference? workModePreference,
        string? skills,
        string? certifications,
        string? portfolioUrl,
        string? positionName)
    {
        CurrentCtc         = currentCtc;
        ExpectedCtc        = expectedCtc;
        NoticePeriodDays   = noticePeriodDays;
        PreferredLocation  = preferredLocation;
        AvailabilityDate   = availabilityDate;
        WillingToRelocate  = willingToRelocate;
        WorkModePreference = workModePreference;
        Skills             = skills;
        Certifications     = certifications;
        PortfolioUrl       = portfolioUrl;
        PositionName       = positionName;
        UpdatedAt = DateTime.UtcNow;
    }
}
