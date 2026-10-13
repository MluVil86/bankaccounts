using BankAccounts.Application.Abstractions.Records;

namespace BankAccounts.Application.Abstractions.Interfaces;

public interface IAccountRepository
{
    Task<IReadOnlyList<DebitedAccount>> TryDebitAsync(long accountId, decimal amount, DateTimeOffset updatedAt, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(long accountId, CancellationToken cancellationToken);
}
