using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Upgrade.Core.Test
{
    public class UpgradeManagerTest
    {
        [Fact]
        public async Task UpgradeToVersion()
        {
            var options = new UpgradeOptions();
            options.TargetVersion = 1;
            options.ConnectionString = "test";
            options.Directory = "./scripts/";

            var manager = new UpgradeManager(
                loggerFactory: new NullLoggerFactory(), 
                dbProvider: new TestDbProvider(),
                 upgradeOptionsAccessor: Options.Create(options)
                );

            await manager.UpgradeToVersionAsync();

            var actualVersion = await manager.GetVersionAsync();

            Assert.Equal(1, actualVersion.Id);
        }
    }
}
