using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using BankAccounts.Application.Outbox;
using BankAccounts.Application.Events;
using BankAccounts.Application.Ledger.Enums;
using BankAccounts.Application.Ledger.Records;
using BankAccounts.Application.Withdrawals.Enums;
using BankAccounts.Application.Withdrawals.Helper;
using BankAccounts.Application.Withdrawals.Commands;
using BankAccounts.Application.Withdrawals.Response;
using BankAccounts.Application.Withdrawals.Validation;
using BankAccounts.Application.Withdrawals.Interfaces;
using BankAccounts.Application.Abstractions.Interfaces;



namespace BankAccounts.Application.Withdrawals.Services;

public sealed class WithdrawalService : IWithdrawalService
{
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<WithdrawalService> _logger;    
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public WithdrawalService(ILogger<WithdrawalService> logger, TimeProvider timeProvider, IUnitOfWorkFactory unitofWorkFactory)
    {
        _logger = logger;
        _timeProvider = timeProvider;    
        _unitOfWorkFactory = unitofWorkFactory;
    }

    public async Task<WithdrawalResult> WithdrawAsync(WithdrawCommand command, CancellationToken cancellationToken)
    {
        if (!WithdrawalAmount.IsValid(command.Amount))
        {
            _logger.LogWarning(
                "Withdrawal declined: invalid amount {Amount} for account {AccountId}. CorrelationId {CorrelationId}",
                command.Amount, command.AccountId, command.CorrelationId);
            return WithdrawalResult.Declined(WithdrawalStatus.InvalidAmount, command.AccountId, command.Amount);
        }

        try
        {
            return await ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException && LogFailure(ex, command))
        {
            throw;
        }
    }
    private async Task<WithdrawalResult> ExecuteAsync(WithdrawCommand command, CancellationToken cancellationToken)
    {
        var requestHash = RequestHasher.Hash(command.AccountId, command.Amount);
        var now = _timeProvider.GetUtcNow();
  
        await using var unitOfWork = await _unitOfWorkFactory.BeginAsync(cancellationToken);

        var claimed = await unitOfWork.Idempotency.TryClaimAsync(
            command.AccountId, command.IdempotencyKey, requestHash, now, cancellationToken);

        if (!claimed)
        {
            return await ReplayAsync(unitOfWork, command, requestHash, cancellationToken);
        }

        var debited = await unitOfWork.Accounts.TryDebitAsync(command.AccountId, command.Amount, now, cancellationToken);

        if (debited.Count == 0)
        {
            return await DeclineAsync(unitOfWork, command, cancellationToken);
        }

        if (debited.Count != 1)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Expected exactly one account row to be updated but {debited.Count} were.");
        }

        var account = debited[0];
        var transactionId = Guid.CreateVersion7(now);
        
        await unitOfWork.Ledger.AppendAsync(
            new LedgerEntry
            {
                TransactionId = transactionId,
                AccountId = command.AccountId,
                EntryType = LedgerEntryType.Withdrawal,
                Amount = command.Amount,
                Currency = account.Currency,
                BalanceAfter = account.NewBalance,
                CorrelationId = command.CorrelationId,
                CreatedAt = now,
            }, cancellationToken);
      
        var withdrawalCompleted = new WithdrawalCompleted
        {
            EventId = Guid.CreateVersion7(now),
            OccurredAt = now,
            TransactionId = transactionId,
            AccountId = command.AccountId,
            Amount = command.Amount,
            Currency = account.Currency,
            CorrelationId = command.CorrelationId,
        };

        await unitOfWork.Outbox.EnqueueAsync(
            new OutboxMessage
            {
                MessageId = withdrawalCompleted.EventId,
                EventType = WithdrawalCompleted.EventType,
                Payload = EventSerializer.Serialize(withdrawalCompleted),
                OccurredAt = now,
            },
            cancellationToken);

        var result = WithdrawalResult.Succeeded(
            command.AccountId, command.Amount, transactionId, account.NewBalance, account.Currency);

        await unitOfWork.Idempotency.SaveOutcomeAsync(
            command.AccountId, command.IdempotencyKey, SerializeOutcome(result), cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Withdrawal {TransactionId} succeeded: {Amount} {Currency} from account {AccountId}, new balance {NewBalance}. CorrelationId {CorrelationId}",
            transactionId, command.Amount, account.Currency, command.AccountId, account.NewBalance, command.CorrelationId);

        return result;
    }

    private async Task<WithdrawalResult> DeclineAsync(
        IUnitOfWork unitOfWork, WithdrawCommand command, CancellationToken cancellationToken)
    {        
        if (!await unitOfWork.Accounts.ExistsAsync(command.AccountId, cancellationToken))
        {            
            await unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogWarning(
                "Withdrawal declined: account {AccountId} not found, amount {Amount}. CorrelationId {CorrelationId}",
                command.AccountId, command.Amount, command.CorrelationId);
            return WithdrawalResult.Declined(WithdrawalStatus.AccountNotFound, command.AccountId, command.Amount);
        }
        
        var result = WithdrawalResult.Declined(WithdrawalStatus.InsufficientFunds, command.AccountId, command.Amount);
        await unitOfWork.Idempotency.SaveOutcomeAsync(
            command.AccountId, command.IdempotencyKey, SerializeOutcome(result), cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        _logger.LogWarning(
            "Withdrawal declined: insufficient funds in account {AccountId} for amount {Amount}. CorrelationId {CorrelationId}",
            command.AccountId, command.Amount, command.CorrelationId);
        return result;
    }
    private async Task<WithdrawalResult> ReplayAsync(
        IUnitOfWork unitOfWork, WithdrawCommand command, byte[] requestHash, CancellationToken cancellationToken)
    {
        var record = await unitOfWork.Idempotency.GetAsync(command.AccountId, command.IdempotencyKey, cancellationToken)
            ?? throw new InvalidOperationException("Idempotency key was reported as existing but could not be read.");
        await unitOfWork.RollbackAsync(cancellationToken);

        if (!record.RequestHash.AsSpan().SequenceEqual(requestHash))
        {
            _logger.LogWarning(
                "Withdrawal declined: idempotency key {IdempotencyKey} reused with a different request for account {AccountId}, amount {Amount}. CorrelationId {CorrelationId}",
                command.IdempotencyKey, command.AccountId, command.Amount, command.CorrelationId);
            return WithdrawalResult.Declined(WithdrawalStatus.IdempotencyKeyReused, command.AccountId, command.Amount);
        }

        var stored = DeserializeOutcome(record.Outcome)
            ?? throw new InvalidOperationException("Idempotency record has no stored outcome.");

        _logger.LogInformation(
            "Withdrawal replayed for idempotency key {IdempotencyKey}: {Status} for account {AccountId}, amount {Amount}. CorrelationId {CorrelationId}",
            command.IdempotencyKey, stored.Status, command.AccountId, command.Amount, command.CorrelationId);
        return stored with { IsReplay = true };
    }
    private bool LogFailure(Exception exception, WithdrawCommand command)
    {
        _logger.LogError(
            exception,
            "Withdrawal failed for account {AccountId}, amount {Amount}, idempotency key {IdempotencyKey}. CorrelationId {CorrelationId}",
            command.AccountId, command.Amount, command.IdempotencyKey, command.CorrelationId);
        return false;
    }

    private static string SerializeOutcome(WithdrawalResult result) =>
       JsonSerializer.Serialize(result, JsonSerializerOptions.Web);

    private static WithdrawalResult? DeserializeOutcome(string? outcome) =>
        outcome is null ? null : JsonSerializer.Deserialize<WithdrawalResult>(outcome, JsonSerializerOptions.Web);
}

