namespace BankAccounts.Application.Abstractions.Interfaces;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> BeginAsync(CancellationToken cancellationToken);
}
