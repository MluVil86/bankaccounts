using BankAccounts.Application.Outbox;

namespace BankAccounts.Application.Abstractions.Interfaces
{
    public interface IOutboxRepository
    {
        Task EnqueueAsync(OutboxMessage message, CancellationToken cancellationToken);
    }
}
