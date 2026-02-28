using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;

namespace LeadFlow.Application.Features.EmailTasks.Commands.BulkScheduleEmail;

public record BulkScheduleEmailCommand(
    List<Guid> LeadIds,
    Guid TemplateId,
    DateTime? ScheduledAt,
    bool StaggerMinutes = true,
    bool ApplyFollowups = true,
    string? OverrideSubject = null,
    string? OverrideBody = null
) : IRequest<BulkScheduleResult>;

public record BulkScheduleResult(int Scheduled, int Skipped, List<string> Errors);

public class BulkScheduleEmailHandler(IMediator mediator)
    : IRequestHandler<BulkScheduleEmailCommand, BulkScheduleResult>
{
    public async Task<BulkScheduleResult> Handle(BulkScheduleEmailCommand cmd, CancellationToken ct)
    {
        var scheduled = 0;
        var skipped = 0;
        var errors = new List<string>();

        var baseTime = cmd.ScheduledAt ?? DateTime.UtcNow;

        for (int i = 0; i < cmd.LeadIds.Count; i++)
        {
            var sendAt = cmd.StaggerMinutes
                ? baseTime.AddMinutes(i)
                : baseTime;

            var result = await mediator.Send(
                new ScheduleEmail.ScheduleEmailCommand(
                    cmd.LeadIds[i], cmd.TemplateId, sendAt, cmd.ApplyFollowups, cmd.OverrideSubject, cmd.OverrideBody),
                ct);

            if (result.IsSuccess) scheduled++;
            else { skipped++; errors.Add(result.Error!); }
        }

        return new BulkScheduleResult(scheduled, skipped, errors);
    }
}
