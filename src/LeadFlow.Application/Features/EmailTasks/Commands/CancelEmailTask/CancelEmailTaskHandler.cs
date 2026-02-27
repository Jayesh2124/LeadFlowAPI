using Hangfire;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Commands.CancelEmailTask;

public record CancelEmailTaskCommand(Guid TaskId) : IRequest<Result>;

public class CancelEmailTaskHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IBackgroundJobClient hangfire)
    : IRequestHandler<CancelEmailTaskCommand, Result>
{
    public async Task<Result> Handle(CancelEmailTaskCommand cmd, CancellationToken ct)
    {
        var task = await db.EmailTasks
            .FirstOrDefaultAsync(t => t.Id == cmd.TaskId && t.UserId == currentUser.UserId, ct);
        if (task is null) return Result.Failure("Email task not found.");

        task.Cancel();

        if (!string.IsNullOrEmpty(task.HangfireJobId))
            hangfire.Delete(task.HangfireJobId);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
