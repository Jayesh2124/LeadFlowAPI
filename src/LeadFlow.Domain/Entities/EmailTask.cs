using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;
using LeadFlow.Domain.Events;
using LeadFlow.Domain.Exceptions;

namespace LeadFlow.Domain.Entities;

public class EmailTask : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid LeadId { get; private set; }
    public Guid TemplateId { get; private set; }
    public Guid? ParentTaskId { get; private set; }        // for followup tasks

    public string RenderedSubject { get; private set; } = default!;
    public string RenderedBody { get; private set; } = default!;

    public DateTime ScheduledAt { get; private set; }
    public EmailTaskStatus Status { get; private set; } = EmailTaskStatus.Scheduled;

    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public DateTime? NextRetryAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public string? HangfireJobId { get; private set; }
    public string? IdempotencyKey { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;
    public Lead Lead { get; private set; } = null!;
    public EmailTemplate Template { get; private set; } = null!;
    public ICollection<EmailAttempt> Attempts { get; private set; } = [];
    public ICollection<EmailFollowup> Followups { get; private set; } = [];

    protected EmailTask() { }

    public static EmailTask Create(
        Guid userId, Guid leadId, Guid templateId,
        string renderedSubject, string renderedBody,
        DateTime scheduledAt, int maxAttempts = 3,
        Guid? parentTaskId = null)
    {
        var task = new EmailTask
        {
            UserId = userId, LeadId = leadId, TemplateId = templateId,
            RenderedSubject = renderedSubject, RenderedBody = renderedBody,
            ScheduledAt = scheduledAt, MaxAttempts = maxAttempts,
            ParentTaskId = parentTaskId,
            IdempotencyKey = $"{userId}:{leadId}:{templateId}:{scheduledAt:yyyyMMddHHmm}"
        };
        task.AddDomainEvent(new EmailTaskScheduledEvent(task.Id));
        return task;
    }

    // ── State Machine ──────────────────────────────────────────

    public void MarkSending()
    {
        GuardStatus(EmailTaskStatus.Scheduled, EmailTaskStatus.Pending, EmailTaskStatus.Sending, EmailTaskStatus.Failed);
        Status = EmailTaskStatus.Sending;
        Touch();
    }

    public void MarkSent()
    {
        GuardStatus(EmailTaskStatus.Sending);
        Status = EmailTaskStatus.Sent;
        SentAt = DateTime.UtcNow;
        AddDomainEvent(new EmailSentEvent(Id, LeadId));
        Touch();
    }

    public void MarkFailed(DateTime? nextRetryAt)
    {
        GuardStatus(EmailTaskStatus.Sending);
        AttemptCount++;
        NextRetryAt = nextRetryAt;

        if (AttemptCount >= MaxAttempts || nextRetryAt is null)
        {
            Status = EmailTaskStatus.Failed;
            AddDomainEvent(new EmailFailedEvent(Id, LeadId));
        }
        else
        {
            Status = EmailTaskStatus.Pending;
        }
        Touch();
    }

    public void Cancel()
    {
        if (Status is EmailTaskStatus.Sent or EmailTaskStatus.Cancelled)
            throw new EmailTaskStateException($"Cannot cancel task in status '{Status}'.");
        Status = EmailTaskStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Touch();
    }

    public void Reschedule(DateTime newTime)
    {
        if (Status is EmailTaskStatus.Sent or EmailTaskStatus.Sending)
            throw new EmailTaskStateException("Cannot reschedule a sent or sending task.");
        ScheduledAt = newTime;
        Status = EmailTaskStatus.Scheduled;
        NextRetryAt = null;
        Touch();
    }

    public void SetHangfireJobId(string jobId) { HangfireJobId = jobId; Touch(); }

    private void GuardStatus(params EmailTaskStatus[] allowed)
    {
        if (!allowed.Contains(Status))
            throw new EmailTaskStateException(
                $"Cannot transition from status '{Status}'. Allowed: {string.Join(", ", allowed)}");
    }
}
