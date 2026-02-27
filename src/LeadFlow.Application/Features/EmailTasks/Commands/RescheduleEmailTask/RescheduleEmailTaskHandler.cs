using FluentValidation;
using Hangfire;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Commands.RescheduleEmailTask;

public record RescheduleEmailTaskCommand(Guid TaskId, DateTime NewScheduledAt) : IRequest<Result>;

public class RescheduleValidator : AbstractValidator<RescheduleEmailTaskCommand>
{
    public RescheduleValidator()
    {
        RuleFor(x => x.NewScheduledAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("New scheduled time must be in the future.");
    }
}

public class RescheduleEmailTaskHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IBackgroundJobClient hangfire)
    : IRequestHandler<RescheduleEmailTaskCommand, Result>
{
    public async Task<Result> Handle(RescheduleEmailTaskCommand cmd, CancellationToken ct)
    {
        var task = await db.EmailTasks
            .FirstOrDefaultAsync(t => t.Id == cmd.TaskId && t.UserId == currentUser.UserId, ct);
        if (task is null) return Result.Failure("Email task not found.");

        // Cancel old Hangfire job
        if (!string.IsNullOrEmpty(task.HangfireJobId))
            hangfire.Delete(task.HangfireJobId);

        task.Reschedule(cmd.NewScheduledAt);

        var delay = cmd.NewScheduledAt - DateTime.UtcNow;
        var jobId = hangfire.Schedule<IEmailTaskProcessor>(
            p => p.ProcessAsync(task.Id, CancellationToken.None),
            delay > TimeSpan.Zero ? delay : TimeSpan.Zero);

        task.SetHangfireJobId(jobId);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
