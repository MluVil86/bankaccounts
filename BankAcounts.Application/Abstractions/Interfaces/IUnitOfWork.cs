namespace BankAccounts.Application.Abstractions.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IAccountRepository Accounts { get; }
    ILedgerRepository Ledger { get; }
    IOutboxRepository Outbox { get; }
    IIdempotencyRepository Idempotency { get; }

    Task CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}
