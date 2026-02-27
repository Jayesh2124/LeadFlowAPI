namespace LeadFlow.Application.Common.Interfaces;

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct = default);
}

public record EmailMessage(
    string ToEmail,
    string ToName,
    string FromEmail,
    string FromName,
    string Subject,
    string HtmlBody,
    List<string>? Attachments = null);

public record EmailSendResult(bool Success, string? SmtpResponse, string? Error, long DurationMs);
