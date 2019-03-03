using System;
using System.IO;
using System.Linq;
using System.Text;
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
        public async Task UpgradeToVersionAsync(int startFromFile = 0) 
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

            int requestedVersion = _options.Version;
            string versionsDirectory = _options.Directory.TrimEnd(Path.DirectorySeparatorChar);
            int currentVersion = 0;
            
            // Get Current version from DB           
            using (_logger.BeginScope("Get current version of schema"))
            {
                _logger.LogInformation("Connecting to database ({ConnectionString})", _options.ConnectionString);
                var currentVersionInfo = await _dbProvider.GetSchemaVersionAsync();
                if (currentVersionInfo != null)
                {
                    currentVersion = currentVersionInfo.Id;
                    _logger.LogInformation("Current Version: {CurrentVersion}", currentVersion);
                    _logger.LogInformation("Last Upgrade: {LastUpgrade}", currentVersionInfo.TimeUTC);
                    _logger.LogInformation("Info: {UpgradeInfo}", currentVersionInfo.Description);
                }
                else
                {
                    _logger.LogWarning("No version info in database.");
                }
            }

            // Load all versions
            var versions = Directory
                .GetDirectories(versionsDirectory)
                .Select(versionDir =>
                {
                    var version = versionDir.Substring(versionDir.LastIndexOf(Path.DirectorySeparatorChar)+1);
                    return Convert.ToInt32(version);
                });

            // Filter version which will run
            var executeVersions = versions
                                .Where(v => v > currentVersion && v <= requestedVersion)
                                .OrderBy(v=>v)
                                .ToList();
            
            _logger.LogInformation("Versions to Run: {VersionRunList}", string.Join(",", executeVersions.Select(v=>v.ToString("000"))));

            // Execute versions
            foreach (var executeVersion in executeVersions)
            {
                using (_logger.BeginScope("Running Version {Version}", executeVersion.ToString("000")))
                {
                    // building full path of the version
                    var fullVersionDir = $"{versionsDirectory}{Path.DirectorySeparatorChar}{executeVersion:000}{Path.DirectorySeparatorChar}";
                    // get all sql files under the version directory
                    var sqlFileNames = Directory
                        .GetFiles(fullVersionDir, "*.sql")
                        .Select(filePath =>
                        {
                            var fileName = filePath.Substring(filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                            return fileName;
                        })
                        .OrderBy(f=>f)
                        .ToList();

                    _logger.LogInformation("STARTED: {Files}", string.Join(",", sqlFileNames));
                    
                    // Run files in version
                    foreach (var fileName in sqlFileNames)
                    {
                        using (_logger.BeginScope("File '{SqlFileName}'", fileName))
                        {                            
                            _logger.LogInformation("Executing");
                            try
                            {
                                var filePath = $"{fullVersionDir}{fileName}";
                                var sql = File.ReadAllText(filePath, Encoding.UTF8);                            
                                await _dbProvider.ExecuteSqlAsync(sql);
                                _logger.LogInformation("Executed successfully.");
                            }
                            catch (Exception exception)
                            {
                                _logger.LogError("Executed with an error '{ExceptionMessage}'", exception.Message);
                                throw new Exception(exception.Message,exception);
                            }
                        }
                    }

                    // update schema version info                    
                    await _dbProvider.SetSchemaVersionAsync(executeVersion);
                    _logger.LogInformation("FINISHED successfully.");
                }
            }
            
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
