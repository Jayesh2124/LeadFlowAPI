using LeadFlow.Application.Features.EmailTasks.Commands.BulkScheduleEmail;
using LeadFlow.Application.Features.EmailTasks.Commands.CancelEmailTask;
using LeadFlow.Application.Features.EmailTasks.Commands.RescheduleEmailTask;
using LeadFlow.Application.Features.EmailTasks.Commands.ScheduleEmail;
using LeadFlow.Application.Features.EmailTasks.Queries.GetLeadTimeline;
using LeadFlow.Application.Features.EmailTasks.Queries.GetLeadDetailsReport;
using LeadFlow.Application.Features.EmailTasks.Queries.GetLeadReport;
using LeadFlow.Application.Features.EmailTasks.Queries.GetRecentEmailTasks;
using LeadFlow.Application.Features.EmailTasks.Queries.GetUserDetailsReport;
using LeadFlow.Application.Features.EmailTasks.Queries.GetUserReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LeadFlow.API.Endpoints;

public static class EmailTaskEndpoints
{
    public static IEndpointRouteBuilder MapEmailTaskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tracking/email/{id:guid}/open", async (Guid id, LeadFlow.Application.Common.Interfaces.IApplicationDbContext db, CancellationToken ct) =>
        {
            var task = await db.EmailTasks.FindAsync(new object[] { id }, ct);
            if (task is not null && task.OpenedAt is null)
            {
                task.MarkOpened();
                await db.SaveChangesAsync(ct);
            }

            // Return a 1x1 transparent GIF pixel
            byte[] pixel = new byte[] {
                0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00,
                0x01, 0x00, 0x80, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44,
                0x01, 0x00, 0x3B
            };

            return Results.File(pixel, "image/gif");
        }).ExcludeFromDescription().AllowAnonymous();

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

        // Lead Reports
        group.MapGet("/reports/leads", async ([FromQuery] string dateFilter, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetLeadReportQuery(dateFilter, startDate, endDate));
            return Results.Ok(result);
        }).WithOpenApi();

        // Lead Details Report
        group.MapGet("/reports/leads/{leadId:guid}/details", async (Guid leadId, [FromQuery] string dateFilter, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetLeadDetailsReportQuery(leadId, dateFilter, startDate, endDate));
            return Results.Ok(result);
        }).WithOpenApi();

        return app;
    }
}

public record RescheduleRequest(DateTime NewScheduledAt);
