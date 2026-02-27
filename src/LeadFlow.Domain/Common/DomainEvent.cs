namespace LeadFlow.Domain.Common;

/// <summary>
/// Base class for domain events. No EF mapping — kept purely in-memory.
/// Never add this to DbContext.
/// </summary>
public abstract class DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
