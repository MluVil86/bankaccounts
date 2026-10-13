using BankAccounts.Application.Ledger.Enums;

namespace BankAccounts.Application.Ledger.Records;

public sealed record LedgerEntry
{
    public required Guid TransactionId { get; init; }
    public required long AccountId { get; init; }
    public required LedgerEntryType EntryType { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required decimal BalanceAfter { get; init; }
    public required string CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
