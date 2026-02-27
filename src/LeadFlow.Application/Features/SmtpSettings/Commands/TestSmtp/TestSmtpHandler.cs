using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.SmtpSettings.Commands.TestSmtp;

public record TestSmtpCommand(
    string Host, int Port, string Username, string Password,
    bool EnableSsl, string FromEmail) : IRequest<Result<string>>;

public class TestSmtpHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    ISmtpConnectionTester tester)
    : IRequestHandler<TestSmtpCommand, Result<string>>

{
    public async Task<Result<string>> Handle(TestSmtpCommand cmd, CancellationToken ct)
    {
        var (success, error) = await tester.TestAsync(
            new SmtpTestConfig(cmd.Host, cmd.Port, cmd.Username,
                               cmd.Password, cmd.EnableSsl, cmd.FromEmail), ct);

        if (!success)
            return Result<string>.Failure(error ?? "Connection failed.");

        // Mark settings as verified if they match stored settings
        var settings = await db.UserSmtpSettings
            .FirstOrDefaultAsync(s => s.UserId == currentUser.UserId, ct);
        if (settings != null && settings.Host == cmd.Host && settings.Username == cmd.Username)
        {
            settings.MarkVerified();
            await db.SaveChangesAsync(ct);
        }

        return Result<string>.Success("SMTP connection successful.");
    }
}
