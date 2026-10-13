using Amazon;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using BankAccounts.Infrastructure.Factory;
using BankAccounts.Infrastructure.Messaging;
using BankAccounts.Infrastructure.Data.Options;
using Microsoft.Extensions.DependencyInjection;
using BankAccounts.Infrastructure.Data.Interfaces;
using BankAccounts.Application.Abstractions.Interfaces;

namespace BankAccounts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {                
        services.AddOptions<DatabaseOptions>()
            .Configure(o => o.ConnectionString = configuration.GetConnectionString(DatabaseOptions.ConnectionStringName) ?? string.Empty)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<SnsOptions>()
            .Bind(configuration.GetSection(SnsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddSingleton<IUnitOfWorkFactory, DapperUnitOfWorkFactory>();       

        services.AddAWSService<IAmazonSimpleNotificationService>((clientConfig, provider) =>
        {
            var sns = provider.GetRequiredService<IOptions<SnsOptions>>().Value;
            clientConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(sns.Region);
            clientConfig.Timeout = sns.Timeout;
            clientConfig.ConnectTimeout = sns.ConnectTimeout;
            clientConfig.RetryMode = RequestRetryMode.Standard;
            clientConfig.MaxErrorRetry = sns.MaxErrorRetry;

            if (!string.IsNullOrWhiteSpace(sns.ServiceUrl))
            {
                clientConfig.ServiceURL = sns.ServiceUrl;
                clientConfig.AuthenticationRegion = sns.Region;
            }
        });

        services.AddSingleton<IEventPublisher, SnsEventPublisher>();        
        return services;
    }
}
