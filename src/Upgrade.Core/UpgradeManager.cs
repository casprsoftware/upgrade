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
                _logger.LogDebug($@"with options:
- {nameof(_options.ConnectionString)}: '{_options.ConnectionString}'
- {nameof(_options.Directory)}: '{_options.Directory}'
- {nameof(_options.TargetVersion)}: {_options.TargetVersion}
- {nameof(_options.StartVersion)}: {_options.StartVersion}
- {nameof(_options.StartFile)}: {_options.StartFile}");

                int targetVersion = _options.TargetVersion;
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

                if (_options.StartVersion.HasValue && _options.StartFile.HasValue)
                {
                    startFromVersion = _options.StartVersion.Value;
                    startFromFile = _options.StartFile.Value;
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

        public async Task<VersionInfo> GetVersionAsync()
        {
            using (_logger.BeginScope("Get Version"))
            {
                ValidateOptions(_options);

                _logger.LogInformation("STARTED.");

                try
                {
                    var versionInfo = await _dbProvider.GetSchemaVersionAsync();
                    if (versionInfo==null)
                    {
                        _logger.LogWarning("No version info.");
                        _logger.LogInformation("DONE.");
                        return null;
                    }
                    _logger.LogInformation("Version: {Version}, Update Time: {Time}", versionInfo.Id, versionInfo.TimeUTC);
                    _logger.LogInformation("DONE.");
                    return versionInfo;
                }
                catch (Exception exception)
                {
                    _logger.LogError("An error: {Error}", exception.Message);
                    throw;
                }
            }
        }

        #endregion

        #region Private Methods

        private void ValidateOptions(UpgradeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.TargetVersion < 0)
            {
                throw new InvalidUpgradeOptionsException(nameof(UpgradeOptions.TargetVersion));
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
