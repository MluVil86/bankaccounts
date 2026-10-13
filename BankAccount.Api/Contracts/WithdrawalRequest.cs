using BankAccounts.Api.Validation;

namespace BankAccounts.Api.Contracts;

public sealed record WithdrawalRequest
{
    [MoneyAmount]
    public required decimal Amount { get; init; }
}
