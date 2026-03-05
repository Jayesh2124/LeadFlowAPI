using LeadFlow.Application.Features.Assignments.Commands;
using LeadFlow.Application.Features.Assignments.Queries;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LeadFlow.API.Endpoints;

public static class AssignmentEndpoints
{
    public static void MapAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        // Assign resource to position
        group.MapPost("/positions/{positionId:guid}/assignments", async (Guid positionId, [FromBody] AssignResourceRequest req, IMediator mediator) =>
        {
            var command = new CreateAssignmentCommand(positionId, req.ResourceId, req.Notes);
            var result = await mediator.Send(command);
            return Results.Ok(new { Id = result });
        })
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Assignments");

        // Get assignments for position
        group.MapGet("/positions/{positionId:guid}/assignments", async (Guid positionId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAssignmentsByPositionQuery(positionId));
            return Results.Ok(result);
        })
        .Produces<List<AssignmentResponse>>(StatusCodes.Status200OK)
        .WithTags("Assignments");

        // Update pipeline stage
        group.MapPatch("/assignments/{assignmentId:guid}/stage", async (Guid assignmentId, [FromBody] UpdateStageRequest req, IMediator mediator) =>
        {
            var command = new UpdateAssignmentStageCommand(assignmentId, req.NewStage);
            await mediator.Send(command);
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Assignments");

        // Remove assignment
        group.MapDelete("/assignments/{assignmentId:guid}", async (Guid assignmentId, IMediator mediator) =>
        {
            await mediator.Send(new DeleteAssignmentCommand(assignmentId));
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Assignments");

        // Create interview
        group.MapPost("/assignments/{assignmentId:guid}/interviews", async (Guid assignmentId, [FromBody] CreateInterviewRequest req, IMediator mediator) =>
        {
            var command = new CreateInterviewCommand(
                assignmentId,
                req.InterviewStage,
                req.InterviewerName,
                req.InterviewerEmail,
                req.ScheduledAt,
                req.EmailBody);

            var result = await mediator.Send(command);
            return Results.Ok(new { Id = result });
        })
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags("Assignments");

        // Get interview history
        group.MapGet("/assignments/{assignmentId:guid}/interviews", async (Guid assignmentId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAssignmentInterviewsQuery(assignmentId));
            return Results.Ok(result);
        })
        .Produces<List<InterviewResponse>>(StatusCodes.Status200OK)
        .WithTags("Assignments");
    }
}

public class AssignResourceRequest
{
    public Guid ResourceId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStageRequest
{
    public AssignmentStage NewStage { get; set; }
}

public class CreateInterviewRequest
{
    public string InterviewStage { get; set; } = default!;
    public string? InterviewerName { get; set; }
    public string? InterviewerEmail { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? EmailBody { get; set; }
}
