using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Upgrade
{
    /// <summary>
    /// Upgrade Manager
    /// </summary>
    public class UpgradeManager
    {
        #region Private Declarations
        
        private readonly ILogger _logger;
        private readonly IDbProvider _dbProvider;
        private readonly UpgradeOptions _options;

        #endregion

        #region Constructor

        public UpgradeManager(
            ILoggerFactory loggerFactory, 
            IDbProvider dbProvider,
            IOptions<UpgradeOptions> upgradeOptionsAccessor)
        {
            _options = upgradeOptionsAccessor.Value;
            _dbProvider = dbProvider;
            _logger = loggerFactory.CreateLogger(typeof(UpgradeManager));
        }

        #endregion

        #region Public Methods

        //
        public Task UpgradeToVersionAsync(int startFromFile = 0) 
        {            
            ValidateOptions(_options);

            _logger.LogInformation("Upgrade Started");
            
            _logger.LogDebug(@"with settings:
- ConnectionString: {ConnectionString}
- Directory: {Directory}
- Version: {Version}
- Start from file: {StartFromFile}", 
                _options.ConnectionString, 
                _options.Directory, 
                _options.Version, startFromFile);
            
            return Task.CompletedTask;
        }

        public Task<VersionInfo> GetVersionAsync()
        {
            ValidateOptions(_options);

            _logger.LogDebug("Get current version of database");

            return _dbProvider.GetSchemaVersionAsync();
        }

        #endregion

        #region Private Methods

        private void ValidateOptions(UpgradeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Version < 0)
            {
                throw new InvalidUpgradeOptionsException(nameof(UpgradeOptions.Version));
            }

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidUpgradeOptionsException(nameof(UpgradeOptions.ConnectionString));
            }

            if (string.IsNullOrWhiteSpace(options.Directory))
            {
                throw new InvalidUpgradeOptionsException(nameof(UpgradeOptions.Directory));
            }
        }

        #endregion
    }
}
