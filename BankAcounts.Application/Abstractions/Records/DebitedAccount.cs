namespace BankAccounts.Application.Abstractions.Records;

public sealed record DebitedAccount(
    decimal NewBalance, 
    string Currency);
