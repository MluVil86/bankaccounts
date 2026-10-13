using BankAccounts.Application.Abstractions.Records;

namespace BankAccounts.Application.Abstractions.Interfaces;

public interface IIdempotencyRepository
{
    Task<bool> TryClaimAsync(long accountId, Guid idempotencyKey, byte[] requestHash, DateTimeOffset createdAt, CancellationToken cancellationToken);

    Task<IdempotencyRecord?> GetAsync(long accountId, Guid idempotencyKey, CancellationToken cancellationToken);

    Task SaveOutcomeAsync(long accountId, Guid idempotencyKey, string outcome, CancellationToken cancellationToken);
}
