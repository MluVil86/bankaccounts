using BankAccounts.Application.Withdrawals.Commands;
using BankAccounts.Application.Withdrawals.Response;

namespace BankAccounts.Application.Withdrawals.Interfaces;

public interface IWithdrawalService
{
    Task<WithdrawalResult> WithdrawAsync(WithdrawCommand command, CancellationToken cancellationToken);
}
