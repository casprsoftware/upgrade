using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Upgrade.SqlRunners
{
    public class DirectorySqlRunner : ISqlRunner
    {
        private readonly ILogger<DirectorySqlRunner> _logger;
        private readonly DirectorySqlRunnerOptions _options;

        public DirectorySqlRunner(
            ILoggerFactory loggerFactory,
            IOptions<DirectorySqlRunnerOptions> optionsAccessor)
        {
            _logger = loggerFactory.CreateLogger<DirectorySqlRunner>();
            _options = optionsAccessor.Value;
        }

        public async Task RunAsync(IDbProvider dbProvider)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("START executing scripts in directory '{directory}'", _options.Directory);
            }

            var files = Directory.GetFiles(_options.Directory, "*.sql");
            
            _logger.LogInformation("Found {filesCount} files in {directory}", files.Length, _options.Directory);

            foreach (var file in files)
            {
                var fileName = file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                _logger.LogInformation("Executing script '{fileName}'", fileName);

                try
                {
                    var sql = File.ReadAllText(file, Encoding.UTF8);
                    await dbProvider.ExecuteSqlAsync(sql);

                    _logger.LogInformation("Executed script '{fileName}'", fileName);
                }
                catch (Exception)
                {
                    _logger.LogError("Script '{fileName}' failed", fileName);
                    throw;
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("FINISH executing scripts in directory '{directory}'", _options.Directory);
            }
        }
    }
}
