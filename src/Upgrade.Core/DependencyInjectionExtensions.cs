using System;
using Microsoft.Extensions.DependencyInjection;

namespace Upgrade
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddUpgrade(
            this IServiceCollection services, 
            Type dbProviderType, Type sqlScriptSourceType)
        {
            services.AddTransient<UpgradeManager>();
            services.AddTransient(typeof(IDbProvider), dbProviderType);
            services.AddTransient(typeof(ISqlScriptSource), sqlScriptSourceType);

            return services;
        }

    }
}