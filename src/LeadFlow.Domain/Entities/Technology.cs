using LeadFlow.Domain.Common;

namespace LeadFlow.Domain.Entities;

public class Technology : BaseEntity
{
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    protected Technology() { }

    public static Technology Create(string name)
        => new() { Name = name, IsActive = true };

    public void Update(string name, bool isActive)
    {
        Name = name;
        IsActive = isActive;
        Touch();
    }
}
