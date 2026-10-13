using System.Text.Json.Serialization;

namespace BankAccounts.Application.Withdrawals.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<WithdrawalStatus>))]
public enum WithdrawalStatus
{
    Succeeded,
    InvalidAmount,
    AccountNotFound,
    InsufficientFunds,
    IdempotencyKeyReused
}
