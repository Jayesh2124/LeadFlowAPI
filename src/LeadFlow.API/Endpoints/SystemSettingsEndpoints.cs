using LeadFlow.Application.Features.SystemSettings.Commands.UpdateSystemSettings;
using LeadFlow.Application.Features.SystemSettings.Queries.GetSystemSettings;
using LeadFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LeadFlow.API.Endpoints;

public static class SystemSettingsEndpoints
{
    public static IEndpointRouteBuilder MapSystemSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/system-settings")
            .WithTags("SystemSettings")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSystemSettingsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithOpenApi();

        group.MapPut("/", async (UpdateSystemSettingsCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        }).WithOpenApi();

        return app;
    }
}
