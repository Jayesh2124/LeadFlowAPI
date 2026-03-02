using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

/// <summary>
/// Represents a specific role/position to be fulfilled within an Opportunity.
/// Each Opportunity (Staffing or Hiring) can have multiple Positions.
/// </summary>
public class OpportunityPosition : BaseEntity
{
    public Guid OpportunityId { get; private set; }

    public string RoleTitle { get; private set; } = default!;
    public int QuantityRequired { get; private set; }

    public int? ExperienceMin { get; private set; }
    public int? ExperienceMax { get; private set; }

    public string? Skills { get; private set; }
    public string? Location { get; private set; }

    public EmploymentType EmploymentType { get; private set; }
    public PositionStatus Status { get; private set; } = PositionStatus.Open;

    // Navigation property
    public Opportunity Opportunity { get; private set; } = null!;

    protected OpportunityPosition() { }

    public static OpportunityPosition Create(
        Guid opportunityId,
        string roleTitle,
        int quantityRequired,
        EmploymentType employmentType,
        int? experienceMin = null,
        int? experienceMax = null,
        string? skills = null,
        string? location = null)
    {
        if (string.IsNullOrWhiteSpace(roleTitle))
            throw new ArgumentException("Role title is required.", nameof(roleTitle));

        if (quantityRequired <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantityRequired), "Quantity required must be greater than zero.");

        return new OpportunityPosition
        {
            OpportunityId = opportunityId,
            RoleTitle = roleTitle.Trim(),
            QuantityRequired = quantityRequired,
            EmploymentType = employmentType,
            ExperienceMin = experienceMin,
            ExperienceMax = experienceMax,
            Skills = skills?.Trim(),
            Location = location?.Trim(),
            Status = PositionStatus.Open
        };
    }

    public void Update(
        string roleTitle,
        int quantityRequired,
        EmploymentType employmentType,
        int? experienceMin = null,
        int? experienceMax = null,
        string? skills = null,
        string? location = null)
    {
        if (string.IsNullOrWhiteSpace(roleTitle))
            throw new ArgumentException("Role title is required.", nameof(roleTitle));

        if (quantityRequired <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantityRequired), "Quantity required must be greater than zero.");

        RoleTitle = roleTitle.Trim();
        QuantityRequired = quantityRequired;
        EmploymentType = employmentType;
        ExperienceMin = experienceMin;
        ExperienceMax = experienceMax;
        Skills = skills?.Trim();
        Location = location?.Trim();

        Touch();
    }

    public void UpdateStatus(PositionStatus status)
    {
        Status = status;
        Touch();
    }
}
