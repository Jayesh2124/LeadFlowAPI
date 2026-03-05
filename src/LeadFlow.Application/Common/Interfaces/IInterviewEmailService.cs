namespace LeadFlow.Application.Common.Interfaces;

public interface IInterviewEmailService
{
    Task SendInterviewEmailsAsync(Guid interviewId, Guid currentUserId, string? emailBody, CancellationToken ct = default);
}
