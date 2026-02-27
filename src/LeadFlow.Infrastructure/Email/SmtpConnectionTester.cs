using LeadFlow.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LeadFlow.Infrastructure.Email;

public class SmtpConnectionTester : ISmtpConnectionTester
{
    public async Task<(bool Success, string? Error)> TestAsync(SmtpTestConfig config, CancellationToken ct)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("LeadFlow Test", config.FromEmail));
            message.To.Add(new MailboxAddress("", config.FromEmail));
            message.Subject = "SMTP Test";

            var builder = new BodyBuilder { TextBody = "This is a connection test from LeadFlow." };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 10_000;

            var secureSocketOptions = config.EnableSsl 
                ? SecureSocketOptions.Auto 
                : SecureSocketOptions.None;

            await client.ConnectAsync(config.Host, config.Port, secureSocketOptions, ct);
            await client.AuthenticateAsync(config.Username, config.Password, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
            
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
