using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LeadFlow.Domain.Entities;

namespace LeadFlow.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserSmtpDto(
    string Host,
    int Port,
    string Username,
    string? Password, // If null, keep existing
    string FromName,
    string FromEmail,
    bool EnableSsl);

public record UpdateUserCommand(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool IsActive,
    UpdateUserSmtpDto? Smtp = null
) : IRequest<Result>;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(300);
        RuleFor(x => x.Role)
            .Must(r => r is "Admin" or "User")
            .WithMessage("Role must be 'Admin' or 'User'.");

        When(x => x.Smtp != null, () => {
            RuleFor(x => x.Smtp!.Host).NotEmpty();
            RuleFor(x => x.Smtp!.Port).GreaterThan(0);
            RuleFor(x => x.Smtp!.Username).NotEmpty();
            RuleFor(x => x.Smtp!.FromEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.Smtp!.FromName).NotEmpty();
        });
    }
}

public class UpdateUserHandler(IApplicationDbContext db, IEncryptionService encryption)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand cmd, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.SmtpSettings)
            .FirstOrDefaultAsync(u => u.Id == cmd.Id, ct);

        if (user is null) return Result.Failure("User not found.");

        var emailTaken = await db.Users
            .AnyAsync(u => u.Email == cmd.Email && u.Id != cmd.Id, ct);
        if (emailTaken)
            return Result.Failure($"Email '{cmd.Email}' is already in use by another user.");

        user.Update(cmd.Name, cmd.Email, cmd.Role, cmd.IsActive);

        if (cmd.Smtp != null)
        {
            if (user.SmtpSettings == null)
            {
                var pwd = encryption.Encrypt(cmd.Smtp.Password ?? "");
                var smtp = UserSmtpSettings.Create(
                    user.Id,
                    cmd.Smtp.Host,
                    cmd.Smtp.Port,
                    cmd.Smtp.Username,
                    pwd,
                    cmd.Smtp.FromName,
                    cmd.Smtp.FromEmail,
                    cmd.Smtp.EnableSsl);
                
                db.UserSmtpSettings.Add(smtp);
                user.SetSmtpSettings(smtp);
            }
            else
            {
                var pwd = !string.IsNullOrEmpty(cmd.Smtp.Password) 
                    ? encryption.Encrypt(cmd.Smtp.Password) 
                    : user.SmtpSettings.EncryptedPassword;

                user.SmtpSettings.Update(
                    cmd.Smtp.Host,
                    cmd.Smtp.Port,
                    cmd.Smtp.Username,
                    pwd,
                    cmd.Smtp.FromName,
                    cmd.Smtp.FromEmail,
                    cmd.Smtp.EnableSsl);
            }
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

