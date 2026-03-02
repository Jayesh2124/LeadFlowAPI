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
    
    // New fields
    public string Country { get; private set; } = "";
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? Address { get; private set; }
    public string? ZipCode { get; private set; }
    public string? Website { get; private set; }
    public List<string> Technologies { get; private set; } = [];

    // Navigation
    public User User { get; private set; } = null!;
    public ICollection<EmailTask> EmailTasks { get; private set; } = [];

    protected Lead() { }

    public static Lead Create(Guid userId, string firstName, string lastName,
        string email, string company, string source, string country = "", string status = "new")
        => new Lead
        {
            UserId = userId, FirstName = firstName, LastName = lastName,
            Email = email, Company = company, Source = source,
            Country = country, Status = status, IsActive = true
        };

    public void Update(string firstName, string lastName, string email,
        string? phone, string company, string? position,
        string status, string source, string? notes, List<string> tags,
        string country, string? city, string? state, string? address,
        string? zipCode, string? website, List<string> technologies)
    {
        FirstName = firstName; LastName = lastName; Email = email;
        Phone = phone; Company = company; Position = position;
        Status = status; Source = source; Notes = notes; Tags = tags;
        Country = country; City = city; State = state; Address = address;
        ZipCode = zipCode; Website = website; Technologies = technologies;
        
        if (Status != "inactive") IsActive = true;
        
        Touch();
    }

    public void Deactivate() { IsActive = false; Status = "inactive"; Touch(); }
    public void Activate() { IsActive = true; Touch(); }

    public void UpdateStatus(string status) { Status = status; Touch(); }

    public string FullName => $"{FirstName} {LastName}";
}
