using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
namespace BankAccounts.Infrastructure.Factory;
using BankAccounts.Infrastructure.Data.Options;
using BankAccounts.Infrastructure.Data.Interfaces;

public sealed class SqlConnectionFactory: ISqlConnectionFactory
{
    private readonly IOptions<DatabaseOptions> _options;
    public SqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options;
    }

    public async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_options.Value.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
            
        }
    }
}
