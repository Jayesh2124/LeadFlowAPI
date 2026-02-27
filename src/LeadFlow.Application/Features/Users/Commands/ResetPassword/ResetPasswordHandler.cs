using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Users.Commands.ResetPassword;

public record ResetPasswordCommand(
    Guid UserId,
    string NewPassword
) : IRequest<Result>;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
    }
}

public class ResetPasswordHandler(IApplicationDbContext db)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == cmd.UserId, ct);
        if (user is null) return Result.Failure("User not found.");

        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(cmd.NewPassword));
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
