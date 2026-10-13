namespace BankAccounts.Application.Outbox;

public sealed record OutboxMessage
{
    public required Guid MessageId { get; init; }
    public required string EventType { get; init; }
    public required string Payload { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
}
