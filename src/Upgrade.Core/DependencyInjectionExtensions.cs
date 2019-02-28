using System;
using Microsoft.Extensions.DependencyInjection;
using Upgrade.Provider;

namespace Upgrade
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddUpgrade(this IServiceCollection services, Action<UpgradeOptions> configure)
        {
            services.AddTransient<UpgradeManager>();
            services.AddTransient<IDbProvider, SqlDbProvider>();
            services.Configure(configure);

            return services;
        }
    }
}