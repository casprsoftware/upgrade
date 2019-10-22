using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Upgrade.DbProviders;
using Upgrade.SqlRunners;

namespace Upgrade
{
    class Program
    {
        public const string FullName = "SQL Upgrade Tool";
        
        static void Main(string[] args)
        {
            Console.Title = FullName;

            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            var app = new CommandLineApplication();
            app.FullName = FullName;
            app.Name = assemblyName.Name + ".dll";            
            app.VersionOption("-v | --version", GetVersion(assemblyName));
            app.HelpOption("-? | -h | --help");
            app.ExtendedHelpText = "\nAuthor: Miro Bozik, https://mirobozik.com";
            
            // OPTIONS
            var directory = app
                .Option("--directory", "Full path of directory with sql scripts.", CommandOptionType.SingleValue)
                .Value();
            var dbProviderOption = app.Option("--db-provider", "Database provider. Default is sql.", CommandOptionType.SingleValue);
            var hostOption = app.Option("--host", "Database host name or IP. Default is localhost.", CommandOptionType.SingleValue);
            var portOption = app.Option("--port", "Database port number.", CommandOptionType.SingleValue);
            var dbNameOption = app.Option("--database", "Database name.", CommandOptionType.SingleValue);
            var userOption = app.Option("--username", "Database username.", CommandOptionType.SingleValue);
            var pwdOption = app.Option("--password", "Database password for the username.", CommandOptionType.SingleValue);
            // ARGS
            var connectionStringArg = app.Argument("connection-string", "Connection string to database. Only for sql provider.");

            app.OnExecute(async () =>
            {
                if (string.IsNullOrEmpty(connectionStringArg.Value)
                    || string.IsNullOrEmpty(directory))
                {
                    app.ShowHelp();
                    return 1;
                }

                var connectionString = connectionStringArg.Value;

                var serviceProvider = BuildServiceProvider(
                    connectionString,
                    directory,
                    true);
                app.Out.WriteLine("Running..");
                var upgradeManager = serviceProvider.GetRequiredService<UpgradeManager>();
                await upgradeManager.RunAsync();

                ((IDisposable)serviceProvider).Dispose();
                return 0;
            });

            if (args != null && args.Length > 0)
            {
                app.Execute(args);
            }
            else
            {
                app.ShowHelp();
            }            
        }

        #region Private Methods

        private static IServiceProvider BuildServiceProvider(
            string connectionString,
            string directory,            
            bool isDebugEnabled
            )
        {            
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
            services.AddUpgrade(typeof(SqlDbProvider), typeof(DirectorySqlRunner));
            services.Configure<DirectorySqlRunnerOptions>(directoryOptions =>
            {
                directoryOptions.Directory = directory;
            });
            services.Configure<SqlDbProviderOptions>(sqlDbProviderOptions =>
            {
                sqlDbProviderOptions.ConnectionString = connectionString;
            });
            return services.BuildServiceProvider();
        }

        private static void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }

        private static string GetVersion(AssemblyName assemblyName)
        {
            return $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}";
        }

        #endregion
    }
}
