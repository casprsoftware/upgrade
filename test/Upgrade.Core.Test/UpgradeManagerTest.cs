using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Upgrade.Core.Test
{
    public class UpgradeManagerTest
    {
        [Fact]
        public async Task UpgradeToVersion_NoVersionInDb()
        {
            var options = new UpgradeOptions
            {
                ConnectionString = "test", 
                Directory = "../../../../../sql/ExampleVersions/".Replace('/','\\')
            };

            var manager = new UpgradeManager(
                loggerFactory: new NullLoggerFactory(), 
                dbProvider: new TestDbProvider(),
                 upgradeOptionsAccessor: Options.Create(options)
                );

            await manager.UpgradeToVersionAsync(targetVersion: 1);

            var actualVersion = await manager.GetVersionAsync();

            Assert.Equal(1, actualVersion.Id);
        }

        [Fact]
        public async Task UpgradeToVersion_WithLatestVersionInDb()
        {
            const int latestVersion = 2;

            var options = new UpgradeOptions
            {
                ConnectionString = "test",
                Directory = "../../../../../sql/ExampleVersions/".Replace('/', '\\')
            };

            var manager = new UpgradeManager(
                loggerFactory: new NullLoggerFactory(),
                dbProvider: new TestDbProvider(new VersionInfo() { Id = latestVersion, TimeUTC = DateTime.UtcNow}),
                upgradeOptionsAccessor: Options.Create(options)
            );

            await manager.UpgradeToVersionAsync();

            var actualVersion = await manager.GetVersionAsync();

            Assert.Equal(latestVersion, actualVersion.Id);
        }

        [Fact]
        public async Task UpgradeToVersion_WithLowerVersionInDb()
        {
            const int versionInDb = 1;
            const int targetVersion = 2;

            var options = new UpgradeOptions
            {
                ConnectionString = "test",
                Directory = "../../../../../sql/ExampleVersions/".Replace('/', '\\')
            };

            var manager = new UpgradeManager(
                loggerFactory: new NullLoggerFactory(),
                dbProvider: new TestDbProvider(new VersionInfo { Id = versionInDb, TimeUTC = DateTime.UtcNow }),
                upgradeOptionsAccessor: Options.Create(options)
            );

            await manager.UpgradeToVersionAsync(targetVersion: targetVersion);

            var actualVersion = await manager.GetVersionAsync();

            Assert.Equal(targetVersion, actualVersion.Id);
        }
    }
}
