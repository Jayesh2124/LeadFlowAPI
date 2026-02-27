using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

/// <summary>Append-only audit log — never updated, only inserted.</summary>
public class EmailAttempt : BaseEntity
{
    public Guid EmailTaskId { get; private set; }
    public int AttemptNumber { get; private set; }
    public EmailAttemptResult Result { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SmtpResponse { get; private set; }
    public DateTime AttemptedAt { get; private set; } = DateTime.UtcNow;
    public long DurationMs { get; private set; }

    public EmailTask EmailTask { get; private set; } = null!;

    protected EmailAttempt() { }

    public static EmailAttempt Success(Guid taskId, int number, string smtpResponse, long durationMs)
        => new() { EmailTaskId = taskId, AttemptNumber = number,
                   Result = EmailAttemptResult.Success, SmtpResponse = smtpResponse,
                   DurationMs = durationMs };

    public static EmailAttempt Failure(Guid taskId, int number, string error, long durationMs)
        => new() { EmailTaskId = taskId, AttemptNumber = number,
                   Result = EmailAttemptResult.Failure, ErrorMessage = error,
                   DurationMs = durationMs };
}
