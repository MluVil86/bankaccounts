namespace BankAccounts.Application.Withdrawals.Validation;

public static class WithdrawalAmount
{
    public const int MaxDecimalPlaces = 2;
    public const decimal MaxValue = 99_999_999_999_999_999.99m;

    public static bool IsValid(decimal amount) =>
        amount > 0 && amount <= MaxValue && decimal.Round(amount, MaxDecimalPlaces) == amount;        
}
