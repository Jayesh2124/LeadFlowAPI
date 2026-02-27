using LeadFlow.Application.Features.Templates.Commands.CreateTemplate;
using LeadFlow.Application.Features.Templates.Commands.DeleteTemplate;
using LeadFlow.Application.Features.Templates.Commands.UpdateTemplate;
using LeadFlow.Application.Features.Templates.Queries.GetTemplates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LeadFlow.API.Endpoints;

public static class TemplateEndpoints
{
    public static IEndpointRouteBuilder MapTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/templates")
            .WithTags("Templates")
            .RequireAuthorization();

        group.MapGet("/", async ([FromQuery] string? search, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTemplatesQuery(search));
            return Results.Ok(result.Value);
        }).WithOpenApi();

        group.MapPost("/", async (CreateTemplateCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess
                ? Results.Created($"/api/templates/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        }).WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateTemplateCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd with { Id = id });
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteTemplateCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).WithOpenApi();

        return app;
    }
}
