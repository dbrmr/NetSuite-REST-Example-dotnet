using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace NetSuiteRESTExample
{
    public static class ServiceProviderBuilder
    {
        public static IServiceProvider GetServiceProvider(string[] args = null)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .AddUserSecrets(typeof(Program).Assembly)
                .AddCommandLine(args)
                .Build();

            var services = new ServiceCollection();
            
            services.AddOptions();
            
            services.Configure<NetSuiteConfig>(configuration.GetSection(nameof(NetSuiteConfig)));
            
            services.AddSingleton<INetSuiteOAuth, NetSuiteOAuth>();

            var provider = services.BuildServiceProvider();
            
            return provider;
        }
    }
}
