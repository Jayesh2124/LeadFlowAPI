namespace LeadFlow.Domain.Enums;

public enum EmailTaskStatus
{
    Scheduled,   // waiting for send time
    Pending,     // waiting for retry
    Sending,     // currently being sent
    Sent,        // successfully delivered
    Failed,      // exhausted all retries
    Cancelled    // manually cancelled
}
