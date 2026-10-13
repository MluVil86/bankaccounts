namespace BankAccounts.Application.Events;

public sealed record WithdrawalCompleted
{
    public const string EventType = "accounts.withdrawal.completed";
    public required Guid EventId { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required Guid TransactionId { get; init; }
    public required long AccountId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string CorrelationId { get; init; }
    public int SchemaVersion { get; init; } = 1;
}
