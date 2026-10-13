using Microsoft.Extensions.DependencyInjection;
using BankAccounts.Application.Withdrawals.Services;
using BankAccounts.Application.Withdrawals.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BankAccounts.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IWithdrawalService, WithdrawalService>();
        return services;
    }
}
