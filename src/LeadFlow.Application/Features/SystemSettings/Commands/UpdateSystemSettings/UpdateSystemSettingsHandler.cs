using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace LeadFlow.Application.Features.SystemSettings.Commands.UpdateSystemSettings;

public record UpdateSystemSettingsCommand(
    int MaxRetries,
    int RetryDelayBaseHours,
    bool AutoFollowup,
    List<LeadFlow.Domain.Entities.FollowupRuleConfig> FollowupRules
) : IRequest<Result>;


public class UpdateSystemSettingsHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSystemSettingsCommand, Result>
{
    public async Task<Result> Handle(UpdateSystemSettingsCommand cmd, CancellationToken ct)
    {
        var settings = await db.SystemSettings.FirstOrDefaultAsync(ct);
        if (settings is null)
        {
            settings = LeadFlow.Domain.Entities.SystemSettings.CreateDefault();

            db.SystemSettings.Add(settings);
        }

        settings.Update(cmd.MaxRetries, cmd.RetryDelayBaseHours,
            cmd.AutoFollowup, cmd.FollowupRules);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
