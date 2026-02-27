using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Users.Commands.CreateUser;

public record CreateUserSmtpDto(
    string Host,
    int Port,
    string Username,
    string Password,
    string FromName,
    string FromEmail,
    bool EnableSsl);

public record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    string Role,   // "Admin" | "User"
    CreateUserSmtpDto? Smtp = null
) : IRequest<Result<Guid>>;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(300);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => r is "Admin" or "User")
            .WithMessage("Role must be 'Admin' or 'User'.");

        When(x => x.Smtp != null, () => {
            RuleFor(x => x.Smtp!.Host).NotEmpty();
            RuleFor(x => x.Smtp!.Port).GreaterThan(0);
            RuleFor(x => x.Smtp!.Username).NotEmpty();
            RuleFor(x => x.Smtp!.Password).NotEmpty();
            RuleFor(x => x.Smtp!.FromEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.Smtp!.FromName).NotEmpty();
        });
    }
}

public class CreateUserHandler(IApplicationDbContext db, IEncryptionService encryption)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var existingUser = await db.Users
            .Include(u => u.SmtpSettings)
            .FirstOrDefaultAsync(u => u.Email == cmd.Email, ct);

        if (existingUser != null)
        {
            existingUser.Update(cmd.Name, cmd.Email, cmd.Role, existingUser.IsActive);
            if (!string.IsNullOrEmpty(cmd.Password))
            {
                existingUser.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(cmd.Password));
            }

            if (cmd.Smtp != null)
            {
                if (existingUser.SmtpSettings == null)
                {
                    var encryptedPwd = encryption.Encrypt(cmd.Smtp.Password ?? "");
                    var smtp = UserSmtpSettings.Create(
                        existingUser.Id,
                        cmd.Smtp.Host,
                        cmd.Smtp.Port,
                        cmd.Smtp.Username,
                        encryptedPwd,
                        cmd.Smtp.FromName,
                        cmd.Smtp.FromEmail,
                        cmd.Smtp.EnableSsl);
                    
                    db.UserSmtpSettings.Add(smtp);
                    existingUser.SetSmtpSettings(smtp);
                }
                else
                {
                    var encryptedPwd = !string.IsNullOrEmpty(cmd.Smtp.Password)
                        ? encryption.Encrypt(cmd.Smtp.Password) 
                        : existingUser.SmtpSettings.EncryptedPassword;

                    existingUser.SmtpSettings.Update(
                        cmd.Smtp.Host,
                        cmd.Smtp.Port,
                        cmd.Smtp.Username,
                        encryptedPwd,
                        cmd.Smtp.FromName,
                        cmd.Smtp.FromEmail,
                        cmd.Smtp.EnableSsl);
                }
            }

            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(existingUser.Id);
        }

        var user = User.Create(
            cmd.Name,
            cmd.Email,
            BCrypt.Net.BCrypt.HashPassword(cmd.Password),
            cmd.Role);

        if (cmd.Smtp != null)
        {
            var encryptedPwd = encryption.Encrypt(cmd.Smtp.Password);
            var smtp = UserSmtpSettings.Create(
                user.Id,
                cmd.Smtp.Host,
                cmd.Smtp.Port,
                cmd.Smtp.Username,
                encryptedPwd,
                cmd.Smtp.FromName,
                cmd.Smtp.FromEmail,
                cmd.Smtp.EnableSsl);
            
            db.UserSmtpSettings.Add(smtp);
            user.SetSmtpSettings(smtp);
        }

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return Result<Guid>.Success(user.Id);
    }
}

