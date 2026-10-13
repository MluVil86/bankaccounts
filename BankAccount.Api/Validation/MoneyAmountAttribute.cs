namespace BankAccounts.Api.Validation;
using System.ComponentModel.DataAnnotations;
using BankAccounts.Application.Withdrawals.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class MoneyAmountAttribute: ValidationAttribute
{
    public MoneyAmountAttribute() :
        base($"The amount must be greater tha zero, have a at most {WithdrawalAmount.MaxDecimalPlaces}, decimal places, and not exceed {WithdrawalAmount.MaxValue}.")
    { 
    }

    public override bool IsValid(object? value) => value is decimal amount && WithdrawalAmount.IsValid(amount);
}
