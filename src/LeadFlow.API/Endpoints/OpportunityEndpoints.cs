using LeadFlow.Application.Features.Opportunities.Commands.ChangeStatus;
using LeadFlow.Application.Features.Opportunities.Commands.Create;
using LeadFlow.Application.Features.Opportunities.Commands.Delete;
using LeadFlow.Application.Features.Opportunities.Commands.Update;
using LeadFlow.Application.Features.Opportunities.DTOs;
using LeadFlow.Application.Features.Opportunities.Queries.GetById;
using LeadFlow.Application.Features.Opportunities.Queries.GetByLead;
using LeadFlow.Application.Features.Opportunities.Queries.GetList;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace LeadFlow.API.Endpoints;

public static class OpportunityEndpoints
{
    public static IEndpointRouteBuilder MapOpportunityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/opportunities")
            .WithTags("Opportunities")
            .RequireAuthorization();

        group.MapPost("/", async ([FromBody] CreateOpportunityRequest request, IMediator mediator) =>
        {
            try
            {
                var command = new CreateOpportunityCommand(request);
                var id = await mediator.Send(command);
                return Results.Created($"/api/opportunities/{id}", new { id });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateOpportunityRequest request, IMediator mediator) =>
        {
            try
            {
                var command = new UpdateOpportunityCommand(id, request);
                await mediator.Send(command);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                var command = new DeleteOpportunityCommand(id);
                await mediator.Send(command);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                var query = new GetOpportunityByIdQuery(id);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.NotFound(new { Error = ex.Message });
            }
        }).WithOpenApi();

        group.MapGet("/", async ([AsParameters] OpportunityFilterRequest filter, IMediator mediator) =>
        {
            try
            {
                var query = new GetOpportunitiesQuery(filter);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).WithOpenApi();

        group.MapPatch("/{id:guid}/status", async (Guid id, [FromBody] ChangeOpportunityStatusRequest request, IMediator mediator) =>
        {
            try
            {
                var command = new ChangeOpportunityStatusCommand(id, request);
                await mediator.Send(command);
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).WithOpenApi();

        // Extra endpoint for /api/leads/{leadId}/opportunities
        app.MapGet("/api/leads/{leadId:guid}/opportunities", async (Guid leadId, IMediator mediator) =>
        {
            try
            {
                var query = new GetOpportunitiesByLeadQuery(leadId);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        })
        .WithTags("Opportunities")
        .RequireAuthorization()
        .WithOpenApi();

        return app;
    }
}
