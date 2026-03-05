using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

/// <summary>
/// Employment history entry for a resource/candidate.
/// </summary>
public class ResourceEmployment : BaseEntity
{
    public Guid ResourceId { get; private set; }

    public string CompanyName { get; private set; } = default!;

    public string Designation { get; private set; } = default!;

    public EmploymentType EmploymentType { get; private set; }

    public DateOnly? StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    /// <summary>True when this is the candidate's current employer.</summary>
    public bool IsCurrent { get; private set; }

    public string? Responsibilities { get; private set; }

    // ── Navigation ─────────────────────────────────────────
    public Resource Resource { get; private set; } = null!;

    protected ResourceEmployment() { }

    public static ResourceEmployment Create(
        Guid resourceId,
        string companyName,
        string designation,
        EmploymentType employmentType,
        DateOnly? startDate = null,
        DateOnly? endDate   = null,
        bool isCurrent      = false,
        string? responsibilities = null)
    {
        return new ResourceEmployment
        {
            ResourceId      = resourceId,
            CompanyName     = companyName,
            Designation     = designation,
            EmploymentType  = employmentType,
            StartDate       = startDate,
            EndDate         = isCurrent ? null : endDate,
            IsCurrent       = isCurrent,
            Responsibilities = responsibilities
        };
    }

    public void Update(
        string companyName,
        string designation,
        EmploymentType employmentType,
        DateOnly? startDate,
        DateOnly? endDate,
        bool isCurrent,
        string? responsibilities)
    {
        CompanyName      = companyName;
        Designation      = designation;
        EmploymentType   = employmentType;
        StartDate        = startDate;
        IsCurrent        = isCurrent;
        EndDate          = isCurrent ? null : endDate;
        Responsibilities = responsibilities;
        Touch();
    }
}
