namespace LeadFlow.Application.Common.Interfaces;

public interface IEmailTaskProcessor
{
    Task ProcessAsync(Guid emailTaskId, CancellationToken ct);
}
