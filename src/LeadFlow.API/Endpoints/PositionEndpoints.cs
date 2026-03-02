using LeadFlow.Application.Features.Positions.Commands.ChangeStatus;
using LeadFlow.Application.Features.Positions.Commands.Create;
using LeadFlow.Application.Features.Positions.Commands.Delete;
using LeadFlow.Application.Features.Positions.Commands.Update;
using LeadFlow.Application.Features.Positions.DTOs;
using LeadFlow.Application.Features.Positions.Queries.GetByOpportunity;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace LeadFlow.API.Endpoints;

public static class PositionEndpoints
{
    public static IEndpointRouteBuilder MapPositionEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Nested under /api/opportunities/{opportunityId}/positions ───────

        var opportunityGroup = app.MapGroup("/api/opportunities/{opportunityId:guid}/positions")
            .WithTags("Positions")
            .RequireAuthorization();

        // POST /api/opportunities/{opportunityId}/positions
        opportunityGroup.MapPost("/", async (
            Guid opportunityId,
            [FromBody] CreatePositionRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreatePositionCommand(opportunityId, request);
            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/positions/{id}", new { id });
        })
        .WithName("CreatePosition")
        .WithSummary("Create a position under an opportunity")
        .WithOpenApi();

        // GET /api/opportunities/{opportunityId}/positions
        opportunityGroup.MapGet("/", async (
            Guid opportunityId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetPositionsByOpportunityQuery(opportunityId);
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .WithName("GetPositionsByOpportunity")
        .WithSummary("Get all positions for an opportunity, sorted by CreatedAt DESC")
        .WithOpenApi();

        // ── Flat /api/positions/{id} group ───────────────────────────────────

        var positionGroup = app.MapGroup("/api/positions")
            .WithTags("Positions")
            .RequireAuthorization();

        // PUT /api/positions/{id}
        positionGroup.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdatePositionRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new UpdatePositionCommand(id, request);
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("UpdatePosition")
        .WithSummary("Update a position")
        .WithOpenApi();

        // DELETE /api/positions/{id}
        positionGroup.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new DeletePositionCommand(id);
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("DeletePosition")
        .WithSummary("Delete a position (not allowed if status = Filled)")
        .WithOpenApi();

        // PATCH /api/positions/{id}/status
        positionGroup.MapPatch("/{id:guid}/status", async (
            Guid id,
            [FromBody] ChangePositionStatusRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new ChangePositionStatusCommand(id, request);
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("ChangePositionStatus")
        .WithSummary("Change the status of a position")
        .WithOpenApi();

        return app;
    }
}
