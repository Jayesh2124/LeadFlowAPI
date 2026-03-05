using LeadFlow.Domain.Common;

namespace LeadFlow.Domain.Entities;

public class AssignmentInterview : BaseEntity
{
    public Guid AssignmentId { get; private set; }

    public string InterviewStage { get; private set; } = default!;

    public string? InterviewerName { get; private set; }
    public string? InterviewerEmail { get; private set; }

    public DateTime ScheduledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public string Status { get; private set; } = default!;

    public string? Feedback { get; private set; }

    // Navigation record
    public ResourceAssignment Assignment { get; private set; } = null!;

    protected AssignmentInterview() { }

    public static AssignmentInterview Create(
        Guid assignmentId,
        string interviewStage,
        DateTime scheduledAt,
        string status,
        string? interviewerName = null,
        string? interviewerEmail = null)
    {
        return new AssignmentInterview
        {
            AssignmentId = assignmentId,
            InterviewStage = interviewStage,
            ScheduledAt = scheduledAt,
            Status = status,
            InterviewerName = interviewerName,
            InterviewerEmail = interviewerEmail
        };
    }

    public void Complete(DateTime completedAt, string status, string? feedback)
    {
        CompletedAt = completedAt;
        Status = status;
        Feedback = feedback;
        Touch();
    }

    public void Reschedule(DateTime newScheduledAt)
    {
        ScheduledAt = newScheduledAt;
        Touch();
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        Touch();
    }
}
