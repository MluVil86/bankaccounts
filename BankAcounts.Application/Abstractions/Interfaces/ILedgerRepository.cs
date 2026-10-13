using BankAccounts.Application.Ledger.Records;

namespace BankAccounts.Application.Abstractions.Interfaces;

public interface ILedgerRepository
{
    Task AppendAsync(LedgerEntry entry, CancellationToken cancellationToken);
}
