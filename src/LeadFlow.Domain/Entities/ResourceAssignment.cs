using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

public class ResourceAssignment : BaseEntity
{
    public Guid PositionId { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid AssignedByUserId { get; private set; }

    public AssignmentStage Stage { get; private set; } = AssignmentStage.Applied;
    public string Status { get; private set; } = default!;

    public string? Notes { get; private set; }

    public DateTime AssignedAt { get; private set; }

    // Navigation properties
    public OpportunityPosition Position { get; private set; } = null!;
    public Resource Resource { get; private set; } = null!;
    public User AssignedByUser { get; private set; } = null!;

    public IReadOnlyCollection<AssignmentInterview> Interviews { get; private set; }
        = new List<AssignmentInterview>();

    public IReadOnlyCollection<AssignmentStageHistory> StageHistories { get; private set; }
        = new List<AssignmentStageHistory>();

    protected ResourceAssignment() { }

    public static ResourceAssignment Create(
        Guid positionId,
        Guid resourceId,
        Guid assignedByUserId,
        string status,
        string? notes = null)
    {
        return new ResourceAssignment
        {
            PositionId = positionId,
            ResourceId = resourceId,
            AssignedByUserId = assignedByUserId,
            Stage = AssignmentStage.Applied,
            Status = status,
            Notes = notes,
            AssignedAt = DateTime.UtcNow
        };
    }

    public void UpdateStage(AssignmentStage newStage, string status)
    {
        Stage = newStage;
        Status = status;
        Touch();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        Touch();
    }
}
