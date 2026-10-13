using Microsoft.Data.SqlClient;
using BankAccounts.Infrastructure.Data.Repository;
using BankAccounts.Application.Abstractions.Interfaces;


namespace BankAccounts.Infrastructure.Factory;

public sealed class DapperUnitOfWork : IUnitOfWork
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;
    private bool _completed;

    public DapperUnitOfWork(SqlConnection connection, SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
        Accounts = new AccountRepository(transaction);
        Ledger = new LedgerRepository(transaction);
        Outbox = new OutboxRepository(transaction);
        Idempotency = new IdempotencyRepository(transaction);
    }

    public IAccountRepository Accounts { get; }

    public ILedgerRepository Ledger { get; }

    public IOutboxRepository Outbox { get; }

    public IIdempotencyRepository Idempotency { get; }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await _transaction.CommitAsync(cancellationToken);
        _completed = true;
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (_completed)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        _completed = true;
    }
}
