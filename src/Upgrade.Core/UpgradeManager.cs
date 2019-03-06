using System;
using System.Collections.Generic;
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
        public async Task UpgradeToVersionAsync() 
        {            
            ValidateOptions(_options);

            using (_logger.BeginScope("Upgrade"))
            {
                _logger.LogDebug(@"with settings:
- ConnectionString: {ConnectionString}
- Directory: {Directory}
- Version: {Version}
- StartFromVersion: {StartFromVersion}
- StartFromFile: {StartFromFile}",
                    _options.ConnectionString,
                    _options.Directory,
                    _options.Version, 
                    _options.StartFromVersion,
                    _options.StartFromFile);

                int targetVersion = _options.Version;
                string versionsDirectory = _options.Directory.TrimEnd(Path.DirectorySeparatorChar);
                int currentVersion;
                int startFromVersion = 1;
                int startFromFile = 1;

                // Get Current version from DB           
                _logger.LogInformation("Checking current version in database");
                var currentVersionInfo = await _dbProvider.GetSchemaVersionAsync();
                if (currentVersionInfo != null)
                {
                    currentVersion = currentVersionInfo.Id;
                    startFromVersion = currentVersion + 1;

                    _logger.LogInformation("Current Version: {CurrentVersion}", currentVersion);
                    _logger.LogInformation("Last Upgrade: {LastUpgrade}", currentVersionInfo.TimeUTC);
                    _logger.LogInformation("Info: {UpgradeInfo}", currentVersionInfo.Description);
                }
                else
                {                    
                    _logger.LogWarning("No version info in database.");
                }

                if (_options.StartFromVersion.HasValue && _options.StartFromFile.HasValue)
                {
                    startFromVersion = _options.StartFromVersion.Value;
                    startFromFile = _options.StartFromFile.Value;
                }

                // Load all versions
                var versions = LoadVersions(versionsDirectory);

                // Filter version which will run
                var executeVersions = versions
                                    .Where(v => v.Id >= startFromVersion && v.Id <= targetVersion)
                                    .OrderBy(v => v.Id)
                                    .ToList();

                _logger.LogInformation(@"Versions to run:
{VersionRunList}", string.Join(", ", executeVersions.Select(v => v.ToString())));

                // Execute versions
                foreach (var executeVersion in executeVersions)
                {
                    using (_logger.BeginScope("Version {Version}", executeVersion))
                    {
                        var files = executeVersion.Files.ToList();
                        if (executeVersion.Id == startFromVersion)
                        {
                            files = executeVersion.Files.Where(f => f.Id >= startFromFile).ToList();
                        }
                        _logger.LogInformation(@"STARTED: 
Files: {Files}", string.Join(", ", files.Select(f=>f.ToString())));

                        // Run files in version
                        foreach (var fileInfo in files)
                        {
                            using (_logger.BeginScope("File '{SqlFileName}'", fileInfo))
                            {
                                _logger.LogInformation("Executing");
                                try
                                {
                                    var sql = File.ReadAllText(fileInfo.FullName, Encoding.UTF8);
                                    await _dbProvider.ExecuteSqlAsync(sql);
                                    _logger.LogInformation("Executed successfully.");
                                }
                                catch (Exception exception)
                                {
                                    _logger.LogError("Executed with an error '{ExceptionMessage}'", exception.Message);
                                    throw new Exception(exception.Message, exception);
                                }
                            }
                        }

                        // update schema version info                    
                        await _dbProvider.SetSchemaVersionAsync(executeVersion.Id);
                        _logger.LogInformation("FINISHED successfully.");
                    }
                }

                _logger.LogInformation("FINISHED.");
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

        private IEnumerable<Version> LoadVersions(string directory)
        {
            var versionIds = Directory
                .GetDirectories(directory)
                .Select(versionDir =>
                {
                    var version = versionDir
                        .Substring(versionDir.LastIndexOf(Path.DirectorySeparatorChar) + 1)
                        .TrimStart('0');
                    return Convert.ToInt32(version);
                });

            foreach (var versionId in versionIds)
            {
                var version = new Version { Id = versionId };

                // building full path of the version
                var fullVersionDir = $"{directory}{Path.DirectorySeparatorChar}{version.Id:000}{Path.DirectorySeparatorChar}";
                
                // get all sql files under the version directory
                version.Files = Directory
                    .GetFiles(fullVersionDir, "*.sql")
                    .Select(filePath =>
                    {
                        var fileName = filePath.Substring(filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                        var fileId = Convert.ToInt32(fileName.Substring(0, fileName.IndexOf('_')).TrimStart('0'));
                        var sqlFile = new SqlFileInfo
                        {
                            Id = fileId,
                            Name = fileName,
                            FullName = filePath
                        };
                        return sqlFile;
                    })
                    .OrderBy(f=>f.Id)
                    .ToList();                

                yield return version;
            }
        }

        #endregion
    }
}
