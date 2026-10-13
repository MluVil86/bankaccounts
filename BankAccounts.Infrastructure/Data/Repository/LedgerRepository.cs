using Dapper;
using Microsoft.Data.SqlClient;
using BankAccounts.Application.Ledger.Records;
using BankAccounts.Application.Abstractions.Interfaces;

namespace BankAccounts.Infrastructure.Data.Repository;

public sealed class LedgerRepository : ILedgerRepository
{
    private const string InsertSql = """
        INSERT INTO dbo.LedgerEntries
            (TransactionId, AccountId, EntryType, Amount, Currency, BalanceAfter, CorrelationId, CreatedAt)
        VALUES
            (@TransactionId, @AccountId, @EntryType, @Amount, @Currency, @BalanceAfter, @CorrelationId, @CreatedAt);
        """;

    private readonly SqlTransaction _transaction;
    public LedgerRepository(SqlTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task AppendAsync(LedgerEntry entry, CancellationToken cancellationToken)
    {
        await _transaction.Connection!.ExecuteAsync(new CommandDefinition(
            InsertSql,
            new
            {
                entry.TransactionId,
                entry.AccountId,
                EntryType = entry.EntryType.ToString(),
                entry.Amount,
                entry.Currency,
                entry.BalanceAfter,
                entry.CorrelationId,
                entry.CreatedAt
            },
            _transaction,
            cancellationToken: cancellationToken));
    }
}
