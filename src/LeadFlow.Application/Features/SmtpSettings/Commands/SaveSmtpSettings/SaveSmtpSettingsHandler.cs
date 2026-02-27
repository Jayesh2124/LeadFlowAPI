using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.SmtpSettings.Commands.SaveSmtpSettings;

public record SaveSmtpSettingsCommand(
    string Host, int Port, string Username, string Password,
    string FromName, string FromEmail, bool EnableSsl = true
) : IRequest<Result>;

public class SaveSmtpSettingsValidator : AbstractValidator<SaveSmtpSettingsCommand>
{
    public SaveSmtpSettingsValidator()
    {
        RuleFor(x => x.Host).NotEmpty();
        RuleFor(x => x.Port).InclusiveBetween(1, 65535);
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FromEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.FromName).NotEmpty().MaximumLength(100);
    }
}

public class SaveSmtpSettingsHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IEncryptionService encryption)
    : IRequestHandler<SaveSmtpSettingsCommand, Result>
{
    public async Task<Result> Handle(SaveSmtpSettingsCommand cmd, CancellationToken ct)
    {
        var encPwd = encryption.Encrypt(cmd.Password);
        var existing = await db.UserSmtpSettings
            .FirstOrDefaultAsync(s => s.UserId == currentUser.UserId, ct);

        if (existing is null)
        {
            db.UserSmtpSettings.Add(UserSmtpSettings.Create(
                currentUser.UserId, cmd.Host, cmd.Port, cmd.Username,
                encPwd, cmd.FromName, cmd.FromEmail, cmd.EnableSsl));
        }
        else
        {
            existing.Update(cmd.Host, cmd.Port, cmd.Username, encPwd,
                cmd.FromName, cmd.FromEmail, cmd.EnableSsl);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
