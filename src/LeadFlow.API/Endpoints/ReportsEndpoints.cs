using LeadFlow.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace LeadFlow.API.Endpoints;

public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reports")
            .RequireAuthorization();

        group.MapGet("/lead-pipeline", async ([AsParameters] GetLeadPipelineReportRequest request, IMediator mediator) =>
        {
            try
            {
                var query = new GetLeadPipelineReportQuery(
                    request.LeadId,
                    request.OpportunityId,
                    request.Stage,
                    request.DateFrom,
                    request.DateTo);
                    
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }).WithOpenApi();

        return app;
    }
}

public class GetLeadPipelineReportRequest
{
    [FromQuery] public Guid? LeadId { get; set; }
    [FromQuery] public Guid? OpportunityId { get; set; }
    [FromQuery] public string? Stage { get; set; }
    [FromQuery] public DateTime? DateFrom { get; set; }
    [FromQuery] public DateTime? DateTo { get; set; }
}
