using Dapper;
using Microsoft.Data.SqlClient;
using BankAccounts.Application.Outbox;
using BankAccounts.Application.Abstractions.Interfaces;

namespace BankAccounts.Infrastructure.Data.Repository;


public sealed class OutboxRepository: IOutboxRepository
{
    private const string InsertSql = """
        INSERT INTO dbo.OutboxMessages (MessageId, EventType, Payload, OccurredAt, NextAttemptAt)
        VALUES (@MessageId, @EventType, @Payload, @OccurredAt, @OccurredAt);
        """;

    private readonly SqlTransaction _transaction;
    public OutboxRepository(SqlTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task EnqueueAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await _transaction.Connection!.ExecuteAsync(new CommandDefinition(
            InsertSql,
            new { message.MessageId, message.EventType, message.Payload, message.OccurredAt },
            _transaction,
            cancellationToken: cancellationToken));
    }
}
