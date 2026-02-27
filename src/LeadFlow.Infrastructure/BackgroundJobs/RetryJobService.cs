using Hangfire;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Enums;
using LeadFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeadFlow.Infrastructure.BackgroundJobs;

/// <summary>Recurring job that rescues Pending tasks whose Hangfire job was lost.</summary>
public class RetryJobService(
    AppDbContext db,
    IBackgroundJobClient hangfire,
    ILogger<RetryJobService> logger)
{
    [Queue("maintenance")]
    public async Task ProcessPendingTasksAsync(CancellationToken ct)
    {
        var staleTasks = await db.EmailTasks
            .Where(t => t.Status == EmailTaskStatus.Pending
                     && t.NextRetryAt.HasValue
                     && t.NextRetryAt <= DateTime.UtcNow)
            .ToListAsync(ct);

        logger.LogInformation("RetryJobService: found {Count} stale pending tasks", staleTasks.Count);

        foreach (var task in staleTasks)
        {
            var jobId = hangfire.Enqueue<IEmailTaskProcessor>(
                p => p.ProcessAsync(task.Id, CancellationToken.None));
            task.SetHangfireJobId(jobId);
        }

        if (staleTasks.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    [Queue("maintenance")]
    public async Task CleanupStaleTasksAsync(CancellationToken ct)
    {
        // Mark as Failed tasks stuck in Sending for > 1 hour (crashed workers)
        var cutoff = DateTime.UtcNow.AddHours(-1);
        var stuck = await db.EmailTasks
            .Where(t => t.Status == EmailTaskStatus.Sending && t.UpdatedAt < cutoff)
            .ToListAsync(ct);

        foreach (var task in stuck)
        {
            // Add failure attempt and exhaust retries
            db.EmailAttempts.Add(Domain.Entities.EmailAttempt.Failure(
                task.Id, task.AttemptCount + 1, "Worker crash detected", 0));
            task.MarkFailed(null);  // null = exhausted
        }

        if (stuck.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogWarning("Cleaned up {Count} stuck tasks", stuck.Count);
        }
    }
}
