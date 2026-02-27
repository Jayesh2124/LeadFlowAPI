using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.SystemSettings.Queries.GetSystemSettings;

public record GetSystemSettingsQuery : IRequest<Result<SystemSettingsDto>>;

public record SystemSettingsDto(
    int MaxRetries,
    int RetryDelayBaseHours,
    bool AutoFollowup,
    List<LeadFlow.Domain.Entities.FollowupRuleConfig> FollowupRules
);

public class GetSystemSettingsHandler(IApplicationDbContext db)
    : IRequestHandler<GetSystemSettingsQuery, Result<SystemSettingsDto>>
{
    public async Task<Result<SystemSettingsDto>> Handle(GetSystemSettingsQuery q, CancellationToken ct)
    {
        var settings = await db.SystemSettings.FirstOrDefaultAsync(ct);
        if (settings is null)
        {
            settings = LeadFlow.Domain.Entities.SystemSettings.CreateDefault();
        }

        return Result<SystemSettingsDto>.Success(new SystemSettingsDto(
            settings.DefaultMaxRetries,
            settings.RetryDelayBaseHours,
            settings.AutoFollowup,
            settings.FollowupRules
        ));
    }
}
