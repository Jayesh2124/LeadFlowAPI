using LeadFlow.Domain.Common;

namespace LeadFlow.Domain.Entities;

public class AssignmentStageHistory : BaseEntity
{
    public Guid AssignmentId { get; private set; }

    public string? PreviousStage { get; private set; }
    public string NewStage { get; private set; } = default!;

    public Guid ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }

    // Navigation properties
    public ResourceAssignment Assignment { get; private set; } = null!;
    public User ChangedByUser { get; private set; } = null!;

    protected AssignmentStageHistory() { }

    public static AssignmentStageHistory Create(
        Guid assignmentId,
        string? previousStage,
        string newStage,
        Guid changedByUserId)
    {
        return new AssignmentStageHistory
        {
            AssignmentId = assignmentId,
            PreviousStage = previousStage,
            NewStage = newStage,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow
        };
    }
}
