using LeadFlow.Application.Features.EmailTasks.Commands.BulkScheduleEmail;
using LeadFlow.Application.Features.EmailTasks.Commands.CancelEmailTask;
using LeadFlow.Application.Features.EmailTasks.Commands.RescheduleEmailTask;
using LeadFlow.Application.Features.EmailTasks.Commands.ScheduleEmail;
using LeadFlow.Application.Features.EmailTasks.Queries.GetLeadTimeline;
using LeadFlow.Application.Features.EmailTasks.Queries.GetRecentEmailTasks;
using LeadFlow.Application.Features.EmailTasks.Queries.GetUserReport;
using LeadFlow.Application.Features.EmailTasks.Queries.GetUserDetailsReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LeadFlow.API.Endpoints;

public static class EmailTaskEndpoints
{
    public static IEndpointRouteBuilder MapEmailTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/email-tasks")
            .WithTags("EmailTasks")
            .RequireAuthorization();

        // Schedule single email
        group.MapPost("/schedule", async (ScheduleEmailCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess
                ? Results.Ok(new { taskId = result.Value })
                : Results.BadRequest(result.Error);
        }).WithOpenApi();

        // Bulk schedule
        group.MapPost("/bulk-schedule", async (BulkScheduleEmailCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return Results.Ok(result);
        }).WithOpenApi();

        // Cancel
        group.MapPut("/{id:guid}/cancel", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CancelEmailTaskCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).WithOpenApi();

        // Reschedule
        group.MapPut("/{id:guid}/reschedule", async (Guid id, RescheduleRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new RescheduleEmailTaskCommand(id, req.NewScheduledAt));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).WithOpenApi();

        // Lead email timeline
        group.MapGet("/lead/{leadId:guid}/timeline", async (Guid leadId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetLeadTimelineQuery(leadId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithOpenApi();

        // Recent tasks
        group.MapGet("/recent", async ([FromQuery] int count, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetRecentEmailTasksQuery(count == 0 ? 10 : count));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithOpenApi();

        // User Reports (Admin)
        group.MapGet("/reports", async ([FromQuery] string dateFilter, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUserReportQuery(dateFilter, startDate, endDate));
            return Results.Ok(result);
        }).WithOpenApi();

        // User Details Report (Admin)
        group.MapGet("/reports/{userId:guid}/details", async (Guid userId, [FromQuery] string dateFilter, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUserDetailsReportQuery(userId, dateFilter, startDate, endDate));
            return Results.Ok(result);
        }).WithOpenApi();

        return app;
    }
}

public record RescheduleRequest(DateTime NewScheduledAt);
