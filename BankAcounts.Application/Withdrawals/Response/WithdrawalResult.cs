using System.Text.Json.Serialization;
using BankAccounts.Application.Withdrawals.Enums;

namespace BankAccounts.Application.Withdrawals.Response;

public sealed record WithdrawalResult
{
    public required WithdrawalStatus Status { get; init; }
    public required long AccountId { get; init; }
    public required decimal Amount { get; init; }
    public Guid? TransactionId { get; init; }
    public decimal? NewBalance { get; init; }
    public string? Currency { get; init; }
    
    [JsonIgnore]
    public bool IsReplay { get; init; }

    public static WithdrawalResult Succeeded(long accountId, decimal amount, Guid transactionId, decimal newBalance, string currency)
    {
        return new()
        {
            Status = WithdrawalStatus.Succeeded,
            AccountId = accountId,
            Amount = amount,
            TransactionId = transactionId,
            NewBalance = newBalance,
            Currency = currency
        }; 
    }

    public static WithdrawalResult Declined(WithdrawalStatus status, long accountId, decimal amount)
    {
        return new()
        {
            Status = status,
            AccountId = accountId,
            Amount = amount
        };
    }
}
