using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Upgrade.Provider;
using Xunit;

namespace Upgrade.Core.Test
{
    public class SqlDbProviderTest
    {
        [Fact]
        public async Task GetVersionInfoAsync()
        {
            var options = new UpgradeOptions
            {
                ConnectionString = "Server=tcp:localhost,1433;Database=WebApp;Persist Security Info=False;User ID=sa;Password=Sa123456#;MultipleActiveResultSets=True;TrustServerCertificate=False;Connection Timeout=30;"
            };

            var provider = new SqlDbProvider(
                optionsAccessor: Options.Create<UpgradeOptions>(options),
                logger: NullLogger<SqlDbProvider>.Instance
            );

            var version = await provider.GetSchemaVersionAsync();

            Assert.Null(version);
        }
    }
}