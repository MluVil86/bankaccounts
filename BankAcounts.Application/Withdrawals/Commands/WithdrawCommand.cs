namespace BankAccounts.Application.Withdrawals.Commands;

public sealed record WithdrawCommand(
    long AccountId,
    decimal Amount,    
    Guid IdempotencyKey,
    string CorrelationId
    );

