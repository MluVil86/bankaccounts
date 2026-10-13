using Dapper;
using Microsoft.Data.SqlClient;
using BankAccounts.Application.Abstractions.Records;
using BankAccounts.Application.Abstractions.Interfaces;

namespace BankAccounts.Infrastructure.Data.Repository;

public sealed class AccountRepository : IAccountRepository
{

    private const string DebitSql = """
    UPDATE dbo.Accounts
    SET    Balance = Balance - @Amount,
           UpdatedAt = @UpdatedAt
    OUTPUT inserted.Balance AS NewBalance, inserted.Currency
    WHERE  Id = @AccountId
      AND  Balance >= @Amount
      AND  @Amount > 0;
    """;

    private const string ExistsSql = """
    SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Accounts WHERE Id = @AccountId) THEN 1 ELSE 0 END;
    """;

    private readonly SqlTransaction _transaction;

    public AccountRepository(SqlTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task<bool> ExistsAsync(long accountId, CancellationToken cancellationToken)
    {
        return await _transaction.Connection!.ExecuteScalarAsync<bool>(new CommandDefinition(
            ExistsSql,
            new { AccountId = accountId },
            _transaction,
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<DebitedAccount>> TryDebitAsync(
        long accountId, decimal amount, DateTimeOffset updatedAt, CancellationToken cancellationToken)
    {
        var rows = await _transaction.Connection!.QueryAsync<DebitedAccount>(new CommandDefinition(
            DebitSql,
            new { AccountId = accountId, Amount = amount, UpdatedAt = updatedAt },
            _transaction,
            cancellationToken: cancellationToken));
        return rows.AsList();
    }
}
