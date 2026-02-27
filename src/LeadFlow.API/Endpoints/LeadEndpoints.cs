using LeadFlow.Application.Features.Leads.Commands.CreateLead;
using LeadFlow.Application.Features.Leads.Commands.DeleteLead;
using LeadFlow.Application.Features.Leads.Commands.UpdateLead;
using LeadFlow.Application.Features.Leads.Queries.GetLeads;
using MediatR;

namespace LeadFlow.API.Endpoints;

public static class LeadEndpoints
{
    public static IEndpointRouteBuilder MapLeadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leads")
            .WithTags("Leads")
            .RequireAuthorization();

        group.MapGet("/", async (
            string? search, string? status, int page = 1, int pageSize = 20,
            IMediator mediator = default!) =>
        {
            var result = await mediator.Send(new GetLeadsQuery(search, status, page, pageSize));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithOpenApi();

        group.MapPost("/", async (CreateLeadCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess
                ? Results.Created($"/api/leads/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        }).WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateLeadCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd with { Id = id });
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteLeadCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).WithOpenApi();

        return app;
    }
}
