using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<Result>;

public class DeleteUserHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand cmd, CancellationToken ct)
    {
        if (cmd.Id == currentUser.UserId)
            return Result.Failure("You cannot delete your own account.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == cmd.Id, ct);
        if (user is null) return Result.Failure("User not found.");

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
