using Microsoft.Data.SqlClient;

namespace BankAccounts.Infrastructure.Data.Interfaces;

public interface ISqlConnectionFactory
{
    Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
