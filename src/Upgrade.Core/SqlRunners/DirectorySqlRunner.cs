using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Upgrade.SqlRunners
{
    public class DirectorySqlRunner : ISqlRunner
    {
        private ILogger<DirectorySqlRunner> _logger;
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
            var files = Directory.GetFiles(_options.Directory, "*.sql");
            foreach (var file in files)
            {
                var sql = File.ReadAllText(file, Encoding.UTF8);
                await dbProvider.ExecuteSqlAsync(sql);
            }
        }
    }
}
