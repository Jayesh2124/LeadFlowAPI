using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

public class Resource : BaseEntity
{
    public Guid UserId { get; private set; }
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Phone { get; private set; }
    public decimal? TotalExperience { get; private set; }
    public string? CurrentLocation { get; private set; }
    public string? Summary { get; private set; }
    public string? Source { get; private set; }
    public ResourceStatus Status { get; private set; } = ResourceStatus.Active;
    public bool IsDeleted { get; private set; } = false;

    // Navigation
    public User User { get; private set; } = null!;

    public IReadOnlyCollection<ResourceEmployment> Employments { get; private set; }
        = new List<ResourceEmployment>();

    public ResourceApplicationDetail? ApplicationDetail { get; private set; }

    public IReadOnlyCollection<ResourceReference> References { get; private set; }
        = new List<ResourceReference>();

    public IReadOnlyCollection<ResourceDocument> Documents { get; private set; }
        = new List<ResourceDocument>();

    protected Resource() { }

    public static Resource Create(
        Guid userId, 
        string fullName, 
        string email, 
        string? phone = null, 
        decimal? totalExperience = null, 
        string? currentLocation = null, 
        string? summary = null, 
        string? source = null, 
        ResourceStatus status = ResourceStatus.Active)
    {
        return new Resource
        {
            UserId = userId,
            FullName = fullName,
            Email = email,
            Phone = phone,
            TotalExperience = totalExperience,
            CurrentLocation = currentLocation,
            Summary = summary,
            Source = source,
            Status = status,
            IsDeleted = false
        };
    }

    public void Update(
        string fullName, 
        string email, 
        string? phone, 
        decimal? totalExperience, 
        string? currentLocation, 
        string? summary, 
        string? source, 
        ResourceStatus status)
    {
        FullName = fullName;
        Email = email;
        Phone = phone;
        TotalExperience = totalExperience;
        CurrentLocation = currentLocation;
        Summary = summary;
        Source = source;
        Status = status;
        
        Touch();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        Touch();
    }

    public void Restore()
    {
        IsDeleted = false;
        Touch();
    }
}
