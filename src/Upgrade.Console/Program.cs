using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            app.ExtendedHelpText = "\nAuthor: Miro Bozik, http://mirobozik.com";

            app.Command("info", config =>
            {
                config.Description = "Get database info (current version, last upgrade time).";
                config.HelpOption("-? | -h | --help");
                var connectionStringArg = config.Argument("connection-string", "Connection string for database connection");
                
                config.OnExecute(async () =>
                {                    
                    if (string.IsNullOrEmpty(connectionStringArg.Value))
                    {
                        PrintError("No connection string found.");
                        app.ShowHelp("info");
                        return 1;
                    }
                    
                    var serviceProvider = BuildServiceProvider(
                        connectionStringArg.Value,
                        null,
                        true);

                    var upgradeManager = serviceProvider.GetRequiredService<UpgradeManager>();
                    var versionInfo = await upgradeManager.GetVersionAsync();
                    if (versionInfo!=null)
                    {
                        Console.WriteLine("version: {0}", versionInfo);
                    }
                    else
                    {
                        Console.WriteLine("No version info found in db.");
                    }
                    return 0;
                });
            });

            app.Command("run", config =>
            {
                config.Description = "Run database upgrade.";
                config.HelpOption("-? | -h | --help");
                                
                var connectionStringArg = config.Argument("connection-string", "Connection string to database");
                var directoryArg = config.Argument("directory", "Directory path with sql scripts");
                var targetVersionArg = config.Argument("target-version", "Target version to upgrade database. Default is latest.");
                var startVersionArg = config.Argument("start-version", "Start version (optional).");
                var startFileArg = config.Argument("start-file", "Start file (optional).");

                config.OnExecute(async () =>
                {
                    if (string.IsNullOrEmpty(connectionStringArg.Value)
                        || string.IsNullOrEmpty(directoryArg.Value))
                    {
                        app.ShowHelp("run");
                        return 1;
                    }

                    var connectionString = connectionStringArg.Value;
                    var directory = directoryArg.Value;
                    var targetVersion = targetVersionArg.ToNullableInt32();
                    var startVersion = startVersionArg.ToNullableInt32();
                    var startFile = startFileArg.ToNullableInt32();
                    
                    var serviceProvider = BuildServiceProvider(
                        connectionString,
                        directory,
                        true);

                    var upgradeManager = serviceProvider.GetRequiredService<UpgradeManager>();
                    await upgradeManager.UpgradeToVersionAsync(
                        targetVersion: targetVersion,
                        startVersion: startVersion,
                        startFile: startFile);

                    return 0;
                });
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
            services.AddUpgrade(options =>
            {
                options.ConnectionString = connectionString;
                options.Directory = directory;
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
