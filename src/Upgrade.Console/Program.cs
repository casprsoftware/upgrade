using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Upgrade
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "Upgrade";

            var provider = BuildServiceProvider(args);
            var options = provider
                .GetRequiredService<IOptions<ConsoleOptions>>()
                .Value;
            var upgradeManager = provider.GetRequiredService<UpgradeManager>();

            try
            {
                //ValidateOptions(options);

                if (options.GetVersionInfo)
                {
                    var versionInfo = upgradeManager
                        .GetVersionAsync()
                        .GetAwaiter()
                        .GetResult();

                    if (versionInfo!=null)
                    {
                        Console.WriteLine("Current version is {0}. Changed at (UTC) {1}", versionInfo.Id, versionInfo.TimeUTC);
                    }
                    else
                    {
                        Console.WriteLine("No version info in database.");
                    }
                    
                }
                else
                {
                    upgradeManager.UpgradeToVersionAsync()
                        .GetAwaiter()
                        .GetResult();
                }
            }
            catch (InvalidUpgradeOptionsException invalidUpgradeOptionsException)
            {
                PrintError(invalidUpgradeOptionsException.Message);                
                PrintUsage();
            }
            catch (Exception exception)
            {
                PrintError(exception.Message);
                PrintUsage();
            }

            ((IDisposable)provider).Dispose();

#if DEBUG
            Console.Read();
#endif
        }

        #region Private Methods

        private static IServiceProvider BuildServiceProvider(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCommandLine(args, new Dictionary<string, string>
            {
                {"-v", "version"},
                {"-c", "connectionString"},
                {"-d", "directory"},
                {"-i", "getVersionInfo"}
            });
            var configuration = configurationBuilder.Build();
            var isDebugEnabled = configuration.GetValue<bool>("debug");

            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddFilter(level =>
                {
                    if (isDebugEnabled && level == LogLevel.Debug)
                    {
                        return true;
                    }

                    if (level == LogLevel.Information || level == LogLevel.Error || level == LogLevel.Warning)
                    {
                        return true;
                    }

                    return false;
                });
                builder.AddConsole(o => o.IncludeScopes = true);
            });
            services.AddUpgrade(options => { configuration.Bind(options); });            
            services.Configure<ConsoleOptions>(o => { configuration.Bind(o); });            

            return services.BuildServiceProvider();
        }

        private static void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }

        private static void PrintUsage()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine("Usage: dotnet {0}.dll [options]", assemblyName);
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine(
@"-v, --version <VERSION>                       Version number you want to upgrade target database. Default is '0.0.0'.");
            Console.WriteLine(
@"-c, --connectionString <CONNECTION STRING>    Connection string to target database you want to upgrade.");
            Console.WriteLine(
@"-d, --directory <DIRECTORY>                   Path to directory with sql scripts. Default is './sql/'.");
            Console.WriteLine(
@"-i, --getVersionInfo <true|false>             Get current version info of database schema. Default is false");
            Console.WriteLine(
@"--debug <true|false>                          Enable debug mode. Default is false.");
            Console.WriteLine();
        }

        

        #endregion
    }
}
