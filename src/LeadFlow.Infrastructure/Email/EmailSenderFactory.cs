using System.Collections.Concurrent;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Infrastructure.Email;

public class EmailSenderFactory(
    AppDbContext db,
    IEncryptionService encryption,
    IBlobStorageService blobStorage) : IEmailSenderFactory
{
    // Cache per-user sender within the Hangfire job scope
    private readonly ConcurrentDictionary<Guid, IEmailSender> _cache = new();

    public async Task<IEmailSender> GetSenderForUserAsync(Guid userId, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(userId, out var cached))
            return cached;

        var settings = await db.UserSmtpSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, ct)
            ?? throw new InvalidOperationException(
                $"No SMTP settings found for user {userId}. Please configure SMTP first.");

        var password = encryption.Decrypt(settings.EncryptedPassword);
        var sender = new SmtpEmailSender(new SmtpConfig(
            settings.Host, settings.Port, settings.Username, password, settings.EnableSsl), blobStorage);

        _cache[userId] = sender;
        return sender;
    }
}
