namespace LeadFlow.Application.Common.Interfaces;

public interface IEmailSenderFactory
{
    Task<IEmailSender> GetSenderForUserAsync(Guid userId, CancellationToken ct = default);
}
