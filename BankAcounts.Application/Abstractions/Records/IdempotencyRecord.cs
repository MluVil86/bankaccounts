namespace BankAccounts.Application.Abstractions.Records;

public record IdempotencyRecord(
    byte[] RequestHash, 
    string? Outcome);