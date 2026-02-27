using LeadFlow.Application.Features.SmtpSettings.Commands.SaveSmtpSettings;
using LeadFlow.Application.Features.SmtpSettings.Commands.TestSmtp;
using MediatR;

namespace LeadFlow.API.Endpoints;

public static class SmtpSettingsEndpoints
{
    public static IEndpointRouteBuilder MapSmtpSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/smtp-settings")
            .WithTags("SmtpSettings")
            .RequireAuthorization();

        group.MapPost("/", async (SaveSmtpSettingsCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        }).WithOpenApi();

        group.MapPost("/test", async (TestSmtpCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithOpenApi();

        return app;
    }
}
