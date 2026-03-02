using LeadFlow.Application.Features.Resources.Commands.Create;
using LeadFlow.Application.Features.Resources.Commands.Delete;
using LeadFlow.Application.Features.Resources.Commands.Update;
using LeadFlow.Application.Features.Resources.DTOs;
using LeadFlow.Application.Features.Resources.Queries.GetById;
using LeadFlow.Application.Features.Resources.Queries.GetList;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace LeadFlow.API.Endpoints;

public static class ResourceEndpoints
{
    public static IEndpointRouteBuilder MapResourceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/resources")
            .WithTags("Resources")
            .RequireAuthorization();

        // POST /api/resources
        group.MapPost("/", async ([FromBody] CreateResourceRequest request, IMediator mediator) =>
        {
            try
            {
                var command = new CreateResourceCommand(request);
                var id = await mediator.Send(command);
                return Results.Created($"/api/resources/{id}", new { id });
            }
            catch (FluentValidation.ValidationException vex)
            {
                var errors = vex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return Results.BadRequest(new { errors });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                return Results.BadRequest(new { Error = msg });
            }
        }).WithOpenApi();

        // GET /api/resources
        group.MapGet("/", async ([AsParameters] ResourceFilterRequest filter, IMediator mediator) =>
        {
            try
            {
                var query = new GetResourcesQuery(filter);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).WithOpenApi();

        // GET /api/resources/{id}
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                var query = new GetResourceByIdQuery(id);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.NotFound(new { Error = ex.Message });
            }
        }).WithOpenApi();

        // PUT /api/resources/{id}
        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateResourceRequest request, IMediator mediator) =>
        {
            try
            {
                var command = new UpdateResourceCommand(id, request);
                await mediator.Send(command);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).WithOpenApi();

        // DELETE /api/resources/{id}
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                var command = new DeleteResourceCommand(id);
                await mediator.Send(command);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).WithOpenApi();

        return app;
    }
}
