using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadFlow.Infrastructure.BackgroundJobs;

public class InterviewEmailService(
    AppDbContext db,
    IEmailSenderFactory senderFactory,
    ILogger<InterviewEmailService> logger) : IInterviewEmailService
{
    public async Task SendInterviewEmailsAsync(Guid interviewId, Guid currentUserId, string? emailBody, CancellationToken ct = default)
    {
        var interview = await db.AssignmentInterviews
            .Include(i => i.Assignment)
            .ThenInclude(a => a.Resource)
            .FirstOrDefaultAsync(i => i.Id == interviewId, ct);

        if (interview == null)
            return;

        var smtp = await db.UserSmtpSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == currentUserId, ct);

        if (smtp == null)
        {
            logger.LogWarning("Cannot send interview emails: User {UserId} has no SMTP settings.", currentUserId);
            return;
        }

        var sender = await senderFactory.GetSenderForUserAsync(currentUserId, ct);
        var resource = interview.Assignment.Resource;

        // Ensure emailBody is not completely null
        var bodyContent = string.IsNullOrWhiteSpace(emailBody) ? 
            $"Your interview for the {interview.InterviewStage} round is scheduled for {interview.ScheduledAt:f}." : emailBody;

        string subject = $"Interview Request: {interview.InterviewStage} - {resource.FullName}";

        // 1. Send to Candidate (Resource)
        if (!string.IsNullOrWhiteSpace(resource.Email))
        {
            var candidateMessage = new EmailMessage(
                resource.Email,
                resource.FullName,
                smtp.FromEmail,
                smtp.FromName,
                $"Your Interview Schedule: {interview.InterviewStage}",
                $"<p>Hi {resource.FullName},</p><p>{bodyContent.Replace("\n", "<br>")}</p>",
                new List<string>());

            await sender.SendAsync(candidateMessage, ct);
        }

        // 2. Send to Interviewer
        if (!string.IsNullOrWhiteSpace(interview.InterviewerEmail))
        {
            var interviewerMessage = new EmailMessage(
                interview.InterviewerEmail,
                interview.InterviewerName ?? "Interviewer",
                smtp.FromEmail,
                smtp.FromName,
                subject,
                $"<p>Hi {interview.InterviewerName},</p><p>You are scheduled to interview <b>{resource.FullName}</b>.</p><p><b>Details:</b></p><p>{bodyContent.Replace("\n", "<br>")}</p>",
                new List<string>());

            await sender.SendAsync(interviewerMessage, ct);
        }
    }
}
