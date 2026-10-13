using BankAccounts.Application.Outbox;

namespace BankAccounts.Application.Abstractions.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken);
}
