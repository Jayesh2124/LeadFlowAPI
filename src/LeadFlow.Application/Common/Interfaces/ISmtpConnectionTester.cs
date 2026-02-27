namespace LeadFlow.Application.Common.Interfaces;

public interface ISmtpConnectionTester
{
    Task<(bool Success, string? Error)> TestAsync(SmtpTestConfig config, CancellationToken ct);
}

public record SmtpTestConfig(
    string Host, int Port,
    string Username, string Password,
    bool EnableSsl, string FromEmail);
