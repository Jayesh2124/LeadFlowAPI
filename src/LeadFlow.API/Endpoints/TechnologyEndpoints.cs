using LeadFlow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.API.Endpoints;

public static class TechnologyEndpoints
{
    public static void MapTechnologyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/technologies")
            .RequireAuthorization()
            .WithTags("Technologies");

        group.MapGet("/", async (IApplicationDbContext db, CancellationToken ct) =>
        {
            var techs = await db.Technologies
                                .Where(t => t.IsActive)
                                .OrderBy(t => t.Name)
                                .Select(t => new { t.Id, t.Name })
                                .ToListAsync(ct);
            return Results.Ok(techs);
        });
    }
}
