namespace BankAccounts.Api.Contracts;

public sealed record WithdrawalResponse(
    Guid TransactionId,
    long AccountId,
    decimal Amount,
    decimal NewBalance,
    string currency);
