using LeadFlow.Domain.Common;

namespace LeadFlow.Domain.Events;

public class EmailTaskScheduledEvent(Guid taskId) : DomainEvent
{
    public Guid TaskId { get; } = taskId;
}

public class EmailSentEvent(Guid taskId, Guid leadId) : DomainEvent
{
    public Guid TaskId { get; } = taskId;
    public Guid LeadId { get; } = leadId;
}

public class EmailFailedEvent(Guid taskId, Guid leadId) : DomainEvent
{
    public Guid TaskId { get; } = taskId;
    public Guid LeadId { get; } = leadId;
}
