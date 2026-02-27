using System.Diagnostics;
using LeadFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LeadFlow.Infrastructure.Email;

public record SmtpConfig(string Host, int Port, string Username, string Password, bool EnableSsl);

public class SmtpEmailSender(SmtpConfig config, IBlobStorageService blobStorage) : IEmailSender
{
    public async Task<EmailSendResult> SendAsync(EmailMessage msg, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(msg.FromName, msg.FromEmail));
            message.To.Add(new MailboxAddress(msg.ToName, msg.ToEmail));
            message.Subject = msg.Subject;

            var builder = new BodyBuilder { HtmlBody = msg.HtmlBody };
            if (msg.Attachments != null && msg.Attachments.Count > 0)
            {
                foreach (var attachment in msg.Attachments)
                {
                    try
                    {
                        var (stream, contentType) = await blobStorage.DownloadAsync(attachment, ct);
                        builder.Attachments.Add(attachment, stream, ContentType.Parse(contentType));
                    }
                    catch (Exception ex)
                    {
                        // Fallback logic for previous local assets or missing blobs
                        var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
                        if (!Directory.Exists(assetsPath))
                            assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
                            
                        var filePath = Path.IsPathRooted(attachment) 
                            ? attachment 
                            : Path.Combine(assetsPath, attachment);

                        if (File.Exists(filePath))
                            builder.Attachments.Add(filePath);
                        else
                            Console.WriteLine($"Attachment not found: {attachment} - {ex.Message}");
                    }
                }
            }
            message.Body = builder.ToMessageBody();
            message.Headers.Add("X-Mailer", "LeadFlow");

            using var client = new SmtpClient();
            client.Timeout = 30_000;

            var secureSocketOptions = config.EnableSsl 
                ? SecureSocketOptions.Auto 
                : SecureSocketOptions.None;

            await client.ConnectAsync(config.Host, config.Port, secureSocketOptions, ct);
            await client.AuthenticateAsync(config.Username, config.Password, ct);

            var response = await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            sw.Stop();
            return new EmailSendResult(true, response, null, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new EmailSendResult(false, null, ex.Message, sw.ElapsedMilliseconds);
        }
    }
}
