using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Infrastructure.Data.Options;

public sealed class DatabaseOptions
{
    public const string ConnectionStringName = "BankAccounts";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
