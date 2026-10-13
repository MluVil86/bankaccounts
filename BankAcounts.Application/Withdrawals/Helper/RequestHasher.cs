using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace BankAccounts.Application.Withdrawals.Helper;

public static class RequestHasher
{
    public static byte[] Hash(long accountId, decimal amount)
    {
        var canonical = string.Create(CultureInfo.InvariantCulture, $"{accountId}:{amount:0.00}");
        return SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
    }
}
