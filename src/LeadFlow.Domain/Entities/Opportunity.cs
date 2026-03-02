using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

public class Opportunity : BaseEntity
{
    public Guid LeadId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid OwnerUserId { get; private set; }

    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    
    public OpportunityType Type { get; private set; }
    public OpportunityStatus Status { get; private set; } = OpportunityStatus.New;
    public OpportunityPriority Priority { get; private set; } = OpportunityPriority.Medium;
    
    public decimal ExpectedValue { get; private set; }
    public DateTime? ExpectedStartDate { get; private set; }
    public DateTime? ExpectedEndDate { get; private set; }
    public bool IsDeleted { get; private set; }


    // Navigation properties
    public Lead Lead { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;
    public User OwnerUser { get; private set; } = null!;
    public ICollection<OpportunityDocument> Documents { get; private set; } = [];
    public ICollection<OpportunityPosition> Positions { get; private set; } = [];

    protected Opportunity() { }

    public static Opportunity Create(
        Guid leadId, 
        Guid createdByUserId, 
        Guid ownerUserId, 
        string title, 
        string? description, 
        OpportunityType type, 
        OpportunityPriority priority,
        decimal expectedValue,
        DateTime? expectedStartDate = null,
        DateTime? expectedEndDate = null)
    {
        return new Opportunity
        {
            LeadId = leadId,
            CreatedByUserId = createdByUserId,
            OwnerUserId = ownerUserId,
            Title = title,
            Description = description,
            Type = type,
            Status = OpportunityStatus.New,
            Priority = priority,
            ExpectedValue = expectedValue,
            ExpectedStartDate = expectedStartDate,
            ExpectedEndDate = expectedEndDate
        };
    }

    public void Update(
        string title, 
        string? description, 
        OpportunityType type, 
        OpportunityStatus status,
        OpportunityPriority priority,
        decimal expectedValue,
        DateTime? expectedStartDate,
        DateTime? expectedEndDate,
        Guid ownerUserId)
    {
        Title = title;
        Description = description;
        Type = type;
        Status = status;
        Priority = priority;
        ExpectedValue = expectedValue;
        ExpectedStartDate = expectedStartDate;
        ExpectedEndDate = expectedEndDate;
        OwnerUserId = ownerUserId;
        
        Touch();
    }

    public void UpdateStatus(OpportunityStatus status)
    {
        Status = status;
        Touch();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        Touch();
    }
}
