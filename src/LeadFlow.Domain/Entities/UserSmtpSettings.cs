using LeadFlow.Domain.Common;

namespace LeadFlow.Domain.Entities;

public class UserSmtpSettings : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Host { get; private set; } = default!;
    public int Port { get; private set; }
    public string Username { get; private set; } = default!;
    public string EncryptedPassword { get; private set; } = default!; // AES-256
    public string FromName { get; private set; } = default!;
    public string FromEmail { get; private set; } = default!;
    public bool EnableSsl { get; private set; } = true;
    public bool IsVerified { get; private set; }

    public User User { get; private set; } = null!;

    protected UserSmtpSettings() { }

    public static UserSmtpSettings Create(Guid userId, string host, int port,
        string username, string encryptedPassword,
        string fromName, string fromEmail, bool enableSsl = true)
        => new UserSmtpSettings
        {
            UserId = userId, Host = host, Port = port,
            Username = username, EncryptedPassword = encryptedPassword,
            FromName = fromName, FromEmail = fromEmail, EnableSsl = enableSsl
        };

    public void Update(string host, int port, string username, string encryptedPassword,
        string fromName, string fromEmail, bool enableSsl)
    {
        Host = host; Port = port; Username = username;
        EncryptedPassword = encryptedPassword;
        FromName = fromName; FromEmail = fromEmail; EnableSsl = enableSsl;
        IsVerified = false;
        Touch();
    }

    public void MarkVerified() { IsVerified = true; Touch(); }
}
