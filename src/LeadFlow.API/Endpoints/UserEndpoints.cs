using LeadFlow.Application.Features.Users.Commands.CreateUser;
using LeadFlow.Application.Features.Users.Commands.DeleteUser;
using LeadFlow.Application.Features.Users.Commands.ResetPassword;
using LeadFlow.Application.Features.Users.Commands.UpdateUser;
using LeadFlow.Application.Features.Users.Queries.GetUserById;
using LeadFlow.Application.Features.Users.Queries.GetUsers;
using MediatR;

namespace LeadFlow.API.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Public: User Registration ────────────────────────────
        // AllowAnonymous so anyone can create an account (or admin can bootstrap)
        app.MapPost("/api/users", async (CreateUserCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .AllowAnonymous()
        .WithTags("Users")
        .WithOpenApi()
        .WithSummary("Create a new user (open registration)");

        // ── Admin-only: User Management ──────────────────────────
        var admin = app
            .MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization("AdminOnly");

        // GET /api/users
        admin.MapGet("/", async (
            IMediator mediator,
            string?   search   = null,
            string?   role     = null,
            bool?     isActive = null,
            int       page     = 1,
            int       pageSize = 20) =>
        {
            var result = await mediator.Send(
                new GetUsersQuery(search, role, isActive, page, pageSize));
            return Results.Ok(result);
        })
        .WithOpenApi()
        .WithSummary("List all users (Admin only)");

        // GET /api/users/{id}
        admin.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var user = await mediator.Send(new GetUserByIdQuery(id));
            return user is null ? Results.NotFound() : Results.Ok(user);
        })
        .WithOpenApi()
        .WithSummary("Get user by ID (Admin only)");

        // PUT /api/users/{id}
        admin.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest req, IMediator mediator) =>
        {
            var cmd    = new UpdateUserCommand(id, req.Name, req.Email, req.Role, req.IsActive, req.Smtp);
            var result = await mediator.Send(cmd);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithOpenApi()
        .WithSummary("Update user details (Admin only)");

        // POST /api/users/{id}/reset-password
        admin.MapPost("/{id:guid}/reset-password",
            async (Guid id, ResetPasswordRequest req, IMediator mediator) =>
        {
            var cmd    = new ResetPasswordCommand(id, req.NewPassword);
            var result = await mediator.Send(cmd);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithOpenApi()
        .WithSummary("Reset user password (Admin only)");

        // DELETE /api/users/{id}
        admin.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteUserCommand(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithOpenApi()
        .WithSummary("Delete a user (Admin only)");

        return app;
    }
}

// ── Request bodies ───────────────────────────────────────────────────────────
public record UpdateUserRequest(
    string Name,
    string Email,
    string Role,
    bool IsActive,
    UpdateUserSmtpDto? Smtp = null);

public record ResetPasswordRequest(string NewPassword);


