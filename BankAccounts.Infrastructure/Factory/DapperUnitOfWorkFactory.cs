using Microsoft.Data.SqlClient;
using BankAccounts.Infrastructure.Data.Interfaces;
using BankAccounts.Application.Abstractions.Interfaces;


namespace BankAccounts.Infrastructure.Factory;

public sealed class DapperUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly ISqlConnectionFactory _connectionFactory;
    public DapperUnitOfWorkFactory(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IUnitOfWork> BeginAsync(CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        try
        {
            var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted, cancellationToken);
            return new DapperUnitOfWork(connection, transaction);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
