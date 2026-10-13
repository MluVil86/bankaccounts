using Dapper;
using Microsoft.Data.SqlClient;
using BankAccounts.Application.Abstractions.Records;
using BankAccounts.Application.Abstractions.Interfaces;

namespace BankAccounts.Infrastructure.Data.Repository;

public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private const int PrimaryKeyViolation = 2627;
    private const int UniqueIndexViolation = 2601;

    private const string ClaimSql = """
        INSERT INTO dbo.IdempotencyRecords (AccountId, IdempotencyKey, RequestHash, CreatedAt)
        VALUES (@AccountId, @IdempotencyKey, @RequestHash, @CreatedAt);
        """;

    private const string SelectSql = """
        SELECT RequestHash, Outcome
        FROM   dbo.IdempotencyRecords
        WHERE  AccountId = @AccountId AND IdempotencyKey = @IdempotencyKey;
        """;

    private const string SaveOutcomeSql = """
        UPDATE dbo.IdempotencyRecords
        SET    Outcome = @Outcome
        WHERE  AccountId = @AccountId AND IdempotencyKey = @IdempotencyKey;
        """;

    private readonly SqlTransaction _transaction;
    public IdempotencyRepository(SqlTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task<bool> TryClaimAsync(
      long accountId, Guid idempotencyKey, byte[] requestHash, DateTimeOffset createdAt, CancellationToken cancellationToken)
    {
        try
        {
            await _transaction.Connection!.ExecuteAsync(new CommandDefinition(
                ClaimSql,
                new { AccountId = accountId, IdempotencyKey = idempotencyKey, RequestHash = requestHash, CreatedAt = createdAt },
                _transaction,
                cancellationToken: cancellationToken));
            return true;
        }
        catch (SqlException ex) when (ex.Number is PrimaryKeyViolation or UniqueIndexViolation)
        {
            return false;
        }
    }

    public async Task<IdempotencyRecord?> GetAsync(long accountId, Guid idempotencyKey, CancellationToken cancellationToken)
    {
        return await _transaction.Connection!.QuerySingleOrDefaultAsync<IdempotencyRecord>(new CommandDefinition(
            SelectSql,
            new { AccountId = accountId, IdempotencyKey = idempotencyKey },
            _transaction,
            cancellationToken: cancellationToken));
    }

    public async Task SaveOutcomeAsync(long accountId, Guid idempotencyKey, string outcome, CancellationToken cancellationToken)
    {
         await _transaction.Connection!.ExecuteAsync(new CommandDefinition(
            SaveOutcomeSql,
            new { AccountId = accountId, IdempotencyKey = idempotencyKey, Outcome = outcome },
            _transaction,
            cancellationToken: cancellationToken));      
    }  
}
