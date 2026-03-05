using LeadFlow.Application.Features.Resources.Commands.ApplicationDetails;
using LeadFlow.Application.Features.Resources.Commands.Create;
using LeadFlow.Application.Features.Resources.Commands.Delete;
using LeadFlow.Application.Features.Resources.Commands.Documents;
using LeadFlow.Application.Features.Resources.Commands.Employment;
using LeadFlow.Application.Features.Resources.Commands.References;
using LeadFlow.Application.Features.Resources.Commands.Update;
using LeadFlow.Application.Features.Resources.DTOs;
using LeadFlow.Application.Features.Resources.Queries.ApplicationDetails;
using LeadFlow.Application.Features.Resources.Queries.Documents;
using LeadFlow.Application.Features.Resources.Queries.Employment;
using LeadFlow.Application.Features.Resources.Queries.GetById;
using LeadFlow.Application.Features.Resources.Queries.GetList;
using LeadFlow.Application.Features.Resources.Queries.References;
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

        // ── Core CRUD ─────────────────────────────────────────────

        group.MapPost("/", async ([FromBody] CreateResourceRequest request, IMediator mediator) =>
        {
            try
            {
                var id = await mediator.Send(new CreateResourceCommand(request));
                return Results.Created($"/api/resources/{id}", new { id });
            }
            catch (FluentValidation.ValidationException vex)
            {
                var errors = vex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return Results.BadRequest(new { errors });
            }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.InnerException?.Message ?? ex.Message }); }
        }).WithOpenApi();

        group.MapGet("/", async ([AsParameters] ResourceFilterRequest filter, IMediator mediator) =>
        {
            try { return Results.Ok(await mediator.Send(new GetResourcesQuery(filter))); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try { return Results.Ok(await mediator.Send(new GetResourceByIdQuery(id))); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.NotFound(new { Error = ex.Message }); }
        }).WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateResourceRequest request, IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new UpdateResourceCommand(id, request));
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new DeleteResourceCommand(id));
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        // ── Employment History ─────────────────────────────────────

        group.MapPost("/{resourceId:guid}/employments",
            async (Guid resourceId, [FromBody] CreateEmploymentRequest request, IMediator mediator) =>
        {
            try
            {
                var id = await mediator.Send(new AddEmploymentCommand(resourceId, request));
                return Results.Created($"/api/employments/{id}", new { id });
            }
            catch (FluentValidation.ValidationException vex)
            {
                var errors = vex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return Results.BadRequest(new { errors });
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        group.MapGet("/{resourceId:guid}/employments",
            async (Guid resourceId, IMediator mediator) =>
        {
            try { return Results.Ok(await mediator.Send(new GetEmploymentsQuery(resourceId))); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.NotFound(new { Error = ex.Message }); }
        }).WithOpenApi();

        // ── Employment (standalone — PUT/DELETE by employment id) ──

        var empGroup = app.MapGroup("/api/employments")
            .WithTags("Employments")
            .RequireAuthorization();

        empGroup.MapPut("/{id:guid}",
            async (Guid id, [FromBody] UpdateEmploymentRequest request, IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new UpdateEmploymentCommand(id, request));
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        empGroup.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new DeleteEmploymentCommand(id));
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        // ── Application Details ────────────────────────────────────

        group.MapGet("/{resourceId:guid}/application",
            async (Guid resourceId, IMediator mediator) =>
        {
            try { return Results.Ok(await mediator.Send(new GetApplicationDetailsQuery(resourceId))); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.NotFound(new { Error = ex.Message }); }
        }).WithOpenApi();

        group.MapPut("/{resourceId:guid}/application",
            async (Guid resourceId, [FromBody] SaveApplicationDetailsRequest request, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new SaveApplicationDetailsCommand(resourceId, request));
                return Results.Ok(result);
            }
            catch (FluentValidation.ValidationException vex)
            {
                var errors = vex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return Results.BadRequest(new { errors });
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        // ── References ─────────────────────────────────────────────

        group.MapPost("/{resourceId:guid}/references",
            async (Guid resourceId, [FromBody] CreateReferenceRequest request, IMediator mediator) =>
        {
            try
            {
                var id = await mediator.Send(new AddReferenceCommand(resourceId, request));
                return Results.Created($"/api/references/{id}", new { id });
            }
            catch (FluentValidation.ValidationException vex)
            {
                var errors = vex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return Results.BadRequest(new { errors });
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        group.MapGet("/{resourceId:guid}/references",
            async (Guid resourceId, IMediator mediator) =>
        {
            try { return Results.Ok(await mediator.Send(new GetReferencesQuery(resourceId))); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.NotFound(new { Error = ex.Message }); }
        }).WithOpenApi();

        var refGroup = app.MapGroup("/api/references")
            .WithTags("References")
            .RequireAuthorization();

        refGroup.MapPut("/{id:guid}",
            async (Guid id, [FromBody] UpdateReferenceRequest request, IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new UpdateReferenceCommand(id, request));
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        refGroup.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new DeleteReferenceCommand(id));
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        // ── Documents ──────────────────────────────────────────────

        group.MapPost("/{resourceId:guid}/documents",
            async (Guid resourceId, [FromBody] UploadDocumentCommand cmd, IMediator mediator) =>
        {
            // The Angular UI uploads the file first via /api/blob/upload, gets the URL back,
            // then calls this endpoint to register the document metadata.
            try
            {
                var command = cmd with { ResourceId = resourceId };
                var id = await mediator.Send(command);
                return Results.Created($"/api/documents/{id}", new { id });
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        group.MapGet("/{resourceId:guid}/documents",
            async (Guid resourceId, IMediator mediator) =>
        {
            try { return Results.Ok(await mediator.Send(new GetDocumentsQuery(resourceId))); }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.NotFound(new { Error = ex.Message }); }
        }).WithOpenApi();

        var docGroup = app.MapGroup("/api/documents")
            .WithTags("Documents")
            .RequireAuthorization();

        docGroup.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new DeleteDocumentCommand(id));
                return Results.NoContent();
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
            catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
        }).WithOpenApi();

        return app;
    }
}
