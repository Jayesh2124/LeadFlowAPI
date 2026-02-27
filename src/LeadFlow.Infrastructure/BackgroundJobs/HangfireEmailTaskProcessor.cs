using System.Diagnostics;
using Hangfire;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Entities;
using LeadFlow.Domain.Enums;
using LeadFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadFlow.Infrastructure.BackgroundJobs;

[AutomaticRetry(Attempts = 0)] // Retry is handled by domain logic
public class HangfireEmailTaskProcessor(
    AppDbContext db,
    IEmailSenderFactory senderFactory,
    IBackgroundJobClient hangfire,
    ILogger<HangfireEmailTaskProcessor> logger)
    : IEmailTaskProcessor
{
    public async Task ProcessAsync(Guid emailTaskId, CancellationToken ct)
    {
        var task = await db.EmailTasks
            .Include(t => t.Lead)
            .Include(t => t.Followups)
            .Include(t => t.Template)
            .FirstOrDefaultAsync(t => t.Id == emailTaskId, ct);

        if (task is null)
        {
            logger.LogWarning("EmailTask {Id} not found", emailTaskId);
            return;
        }

        // Idempotency guard
        if (task.Status is EmailTaskStatus.Sent or EmailTaskStatus.Cancelled)
        {
            logger.LogInformation("Skipping task {Id} — already {Status}", emailTaskId, task.Status);
            return;
        }

        // Transition to Sending
        task.MarkSending();
        await db.SaveChangesAsync(ct);

        // ── Send ────────────────────────────────────
        var sw = Stopwatch.StartNew();
        try
        {
            var smtp = await db.UserSmtpSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == task.UserId, ct)
                ?? throw new InvalidOperationException("SMTP settings not found.");

            var sender = await senderFactory.GetSenderForUserAsync(task.UserId, ct);

            var message = new Application.Common.Interfaces.EmailMessage(
                task.Lead.Email, task.Lead.FullName,
                smtp.FromEmail, smtp.FromName,
                task.RenderedSubject, task.RenderedBody,
                task.Template?.Attachments ?? []);

            var result = await sender.SendAsync(message, ct);
            sw.Stop();

            if (result.Success)
            {
                db.EmailAttempts.Add(
                    EmailAttempt.Success(task.Id, task.AttemptCount + 1,
                                         result.SmtpResponse!, sw.ElapsedMilliseconds));
                task.MarkSent();
                await db.SaveChangesAsync(ct);

                await ScheduleFollowupsAsync(task, ct);
                logger.LogInformation("EmailTask {Id} sent successfully", emailTaskId);
            }
            else
            {
                db.EmailAttempts.Add(
                    EmailAttempt.Failure(task.Id, task.AttemptCount + 1,
                                         result.Error!, sw.ElapsedMilliseconds));
                var retry = ComputeRetry(task.AttemptCount, task.MaxAttempts);
                task.MarkFailed(retry);
                await db.SaveChangesAsync(ct);

                if (task.Status == EmailTaskStatus.Pending && retry.HasValue)
                {
                    var jobId = hangfire.Schedule<IEmailTaskProcessor>(
                        p => p.ProcessAsync(task.Id, CancellationToken.None),
                        retry.Value - DateTime.UtcNow);
                    task.SetHangfireJobId(jobId);
                    await db.SaveChangesAsync(ct);
                }
                logger.LogWarning("EmailTask {Id} failed: {Error}", emailTaskId, result.Error);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Unexpected error processing EmailTask {Id}", emailTaskId);

            db.EmailAttempts.Add(
                EmailAttempt.Failure(task.Id, task.AttemptCount + 1,
                                     ex.Message, sw.ElapsedMilliseconds));
            var retry = ComputeRetry(task.AttemptCount, task.MaxAttempts);
            task.MarkFailed(retry);
            await db.SaveChangesAsync(ct);

            if (task.Status == EmailTaskStatus.Pending && retry.HasValue)
            {
                var jobId = hangfire.Schedule<IEmailTaskProcessor>(
                    p => p.ProcessAsync(task.Id, CancellationToken.None),
                    retry.Value - DateTime.UtcNow);
                task.SetHangfireJobId(jobId);
                await db.SaveChangesAsync(ct);
            }
        }
    }

    /// <summary>Exponential backoff: 1h, 2h, 4h... Returns null when exhausted.</summary>
    private static DateTime? ComputeRetry(int attemptCount, int maxAttempts)
    {
        if (attemptCount + 1 >= maxAttempts) return null;
        return DateTime.UtcNow.AddHours(Math.Pow(2, attemptCount));
    }

    private async Task ScheduleFollowupsAsync(EmailTask task, CancellationToken ct)
    {
        var pending = task.Followups
            .Where(f => f.IsEnabled && !f.Generated)
            .OrderBy(f => f.Order);

        foreach (var followup in pending)
        {
            // Only Always condition is auto-generated; IfNotOpened/IfNotReplied require open tracking
            if (followup.Condition != FollowupCondition.Always) continue;

            var sendAt = DateTime.UtcNow.AddDays(followup.DelayDays);
            
            // Deliver same mail with subject suffix
            var subject = $"{task.RenderedSubject} - Follow up {followup.Order}";
            var body    = task.RenderedBody;

            var childTask = EmailTask.Create(
                task.UserId, task.LeadId, followup.TemplateId,
                subject, body, sendAt, task.MaxAttempts, parentTaskId: task.Id);

            db.EmailTasks.Add(childTask);
            followup.MarkGenerated();

            var jobId = hangfire.Schedule<IEmailTaskProcessor>(
                p => p.ProcessAsync(childTask.Id, CancellationToken.None),
                sendAt - DateTime.UtcNow);
            childTask.SetHangfireJobId(jobId);
        }

        await db.SaveChangesAsync(ct);
    }

}
