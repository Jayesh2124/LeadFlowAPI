using LeadFlow.Domain.Common;

namespace LeadFlow.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Role { get; private set; } = default!;   // "Admin" | "User"
    public bool IsActive { get; private set; } = true;

    // Navigation
    public UserSmtpSettings? SmtpSettings { get; private set; }
    public ICollection<Lead> Leads { get; private set; } = [];
    public ICollection<EmailTemplate> Templates { get; private set; } = [];
    public ICollection<EmailTask> EmailTasks { get; private set; } = [];

    protected User() { }

    public static User Create(string name, string email, string passwordHash, string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return new User { Name = name, Email = email, PasswordHash = passwordHash, Role = role };
    }

    public void Update(string name, string email, string role, bool isActive)
    {
        Name     = name;
        Email    = email;
        Role     = role;
        IsActive = isActive;
        Touch();
    }

    public void SetPasswordHash(string hash) { PasswordHash = hash; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate()   { IsActive = true;  Touch(); }

    public void SetSmtpSettings(UserSmtpSettings settings)
    {
        SmtpSettings = settings;
        Touch();
    }
}


