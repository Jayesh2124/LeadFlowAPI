using LeadFlow.Domain.Common;

namespace LeadFlow.Domain.Entities;

public class Lead : BaseEntity
{
    public Guid UserId { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string Company { get; private set; } = default!;
    public string? Position { get; private set; }
    public string Status { get; private set; } = "new"; // new|contacted|qualified|converted|lost
    public string Source { get; private set; } = default!;
    public string? Notes { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public bool IsActive { get; private set; } = true;

    // Navigation
    public User User { get; private set; } = null!;
    public ICollection<EmailTask> EmailTasks { get; private set; } = [];

    protected Lead() { }

    public static Lead Create(Guid userId, string firstName, string lastName,
        string email, string company, string source, string status = "new")
        => new Lead
        {
            UserId = userId, FirstName = firstName, LastName = lastName,
            Email = email, Company = company, Source = source, Status = status,
            IsActive = true
        };

    public void Update(string firstName, string lastName, string email,
        string? phone, string company, string? position,
        string status, string source, string? notes, List<string> tags)
    {
        FirstName = firstName; LastName = lastName; Email = email;
        Phone = phone; Company = company; Position = position;
        Status = status; Source = source; Notes = notes; Tags = tags;
        
        if (Status != "inactive") IsActive = true;
        
        Touch();
    }

    public void Deactivate() { IsActive = false; Status = "inactive"; Touch(); }
    public void Activate() { IsActive = true; Touch(); }

    public void UpdateStatus(string status) { Status = status; Touch(); }

    public string FullName => $"{FirstName} {LastName}";
}
