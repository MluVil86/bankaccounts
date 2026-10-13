using BankAccounts.Api.Contracts;
using BankAccounts.Api.Middleware;
using BankAccounts.Api.Validation;
using BankAccounts.Application.Withdrawals.Commands;
using BankAccounts.Application.Withdrawals.Enums;
using BankAccounts.Application.Withdrawals.Interfaces;
using BankAccounts.Application.Withdrawals.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BankAccounts.Api.Controllers;

[ApiController]
[Route("accounts/{accountId:long}/withdrawals")]
[Produces("application/json")]
public sealed class AccountWithdrawalController : ControllerBase
{
    public const string IdempotencyKeyHeader = "Idempotency-Key";
    public const string IdempotentReplayedHeader = "Idempotency-Replayed";
    private readonly IWithdrawalService _withdrawalService;    
    public AccountWithdrawalController(IWithdrawalService withdrawalService)
    {
        _withdrawalService = withdrawalService;        
    }

    [HttpPost]
    [ProducesResponseType<WithdrawalResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Withdraw(
        [FromRoute, Range(1, long.MaxValue)] long accountId,
        [FromHeader(Name = IdempotencyKeyHeader), Required] Guid? idempotencyKey,
        [FromBody] WithdrawalRequest request,
        CancellationToken cancellationToken)
    {        
        if (idempotencyKey == Guid.Empty)
        {
            ModelState.AddModelError(IdempotencyKeyHeader, "The Idempotency-Key header must be a non-empty GUID.");
            return ValidationProblem(ModelState);
        }

        var command = new WithdrawCommand(
            accountId, request.Amount, idempotencyKey!.Value, CorrelationIdMiddleware.Get(HttpContext));

        var result = await _withdrawalService.WithdrawAsync(command, cancellationToken);

        if (result.IsReplay)
        {
            Response.Headers[IdempotentReplayedHeader] = "true";
        }

        return ToActionResult(result);
    }     

    private IActionResult ToActionResult(WithdrawalResult result)
    {
        switch (result.Status)
        {
            case WithdrawalStatus.Succeeded:
                return Created(
                    $"/accounts/{result.AccountId}/withdrawals/{result.TransactionId}",
                    new WithdrawalResponse(
                    result.TransactionId!.Value, result.AccountId, result.Amount, result.NewBalance!.Value, result.Currency!));
                  
            case WithdrawalStatus.InvalidAmount:
                return InvalidAmountProblem();                

            case WithdrawalStatus.AccountNotFound:
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Account not found.",
                    detail: $"Account {result.AccountId} does not exist.");
                

            case WithdrawalStatus.InsufficientFunds:
                return Problem(
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Insufficient funds.",
                    detail: $"Account {result.AccountId} has insufficient funds for a withdrawal of {result.Amount}.");                

            case WithdrawalStatus.IdempotencyKeyReused:
                return Problem(
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    title: "Idempotency key reused.",
                    detail: $"The {IdempotencyKeyHeader} has already been used with a different request.");               
            default:
                throw new UnreachableException($"Unhandled withdrawal status {result.Status}.");
        }
    }

    private ActionResult InvalidAmountProblem()
    {
        ModelState.AddModelError(nameof(WithdrawalRequest.Amount), new MoneyAmountAttribute().FormatErrorMessage(nameof(WithdrawalRequest.Amount)));
        return ValidationProblem(ModelState);
    }
}
