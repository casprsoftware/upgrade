using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Upgrade.DbProviders;
using Upgrade.SqlScriptSources;

namespace Upgrade
{
    class Program
    {
        private const string FullName = "SQL Upgrade Tool";
        
        static void Main(string[] args)
        {
            Console.Title = FullName;

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var version = $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}";
            var app = new CommandLineApplication
            {
                FullName = FullName,
                Name = assemblyName.Name + ".exe",
                ExtendedHelpText = "\nAuthor: CASPR Software, https://casprsoftware.com"
            };
            app.VersionOption("-v | --version", version);
            app.HelpOption("-? | -h | --help");

            // OPTIONS
            var isDebugOption = app.Option("--debug", "Enable debug logging.", CommandOptionType.NoValue);
            var directoryOption = app.Option("--directory", "Full path of directory with sql scripts.", CommandOptionType.SingleValue);
            var hostOption = app.Option("--host", "Database host name or IP. Default is localhost.", CommandOptionType.SingleValue);
            var portOption = app.Option("--port", "Database port number.", CommandOptionType.SingleValue);
            var dbNameOption = app.Option("--database", "Database name.", CommandOptionType.SingleValue);
            var userOption = app.Option("--username", "Database username.", CommandOptionType.SingleValue);
            var pwdOption = app.Option("--password", "Database password for the username.", CommandOptionType.SingleValue);
            
            // ARGS
            var connectionStringArg = app
                                    .Argument("connection-string", "Connection string to database. Only for sql provider.")
                                    ;

            app.OnExecute(async () =>
            {
                if (string.IsNullOrEmpty(connectionStringArg.Value)
                    || string.IsNullOrEmpty(directoryOption.Value()))
                {
                    app.ShowHelp();
                    return 1;
                }

                app.Out.WriteLine($"{FullName} {version}");
                app.Out.WriteLine("");
                app.Out.WriteLine("> directory: " + directoryOption.Value());
                app.Out.WriteLine("> provider: sql");

                var services = new ServiceCollection();
                services.AddLogging(builder =>
                {
                    builder.AddFilter(level =>
                    {
                        if (!string.IsNullOrEmpty(isDebugOption.Value()) && level == LogLevel.Debug)
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
                services.AddUpgrade(typeof(SqlDbProvider), typeof(DirectorySqlScriptSource));
                services.Configure<DirectorySqlScriptSourceOptions>(directoryOptions =>
                {
                    directoryOptions.Directory = directoryOption.Value();
                });
                services.Configure<SqlDbProviderOptions>(sqlDbProviderOptions =>
                {
                    sqlDbProviderOptions.ConnectionString = connectionStringArg.Value;
                    sqlDbProviderOptions.Username = userOption.Value();
                    sqlDbProviderOptions.Password = pwdOption.Value();
                    sqlDbProviderOptions.Database = dbNameOption.Value();
                    if (!string.IsNullOrEmpty(hostOption.Value()))
                    {
                        sqlDbProviderOptions.Host = hostOption.Value();
                    }

                    var port = portOption.Value();
                    if (!string.IsNullOrEmpty(port) && int.TryParse(port, out var portNumber))
                    {
                        sqlDbProviderOptions.Port = portNumber;
                    }
                });
                var serviceProvider = services.BuildServiceProvider();

                var upgradeManager = serviceProvider.GetRequiredService<UpgradeManager>();
                app.Out.WriteLine("Running..");
                await upgradeManager.RunAsync();
                app.Out.WriteLine("Done");
                serviceProvider.Dispose();
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
    }
}
