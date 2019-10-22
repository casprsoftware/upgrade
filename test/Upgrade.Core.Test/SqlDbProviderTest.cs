using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Upgrade.DbProviders;
using Xunit;

namespace Upgrade.Core.Test
{
    public class SqlDbProviderTest
    {
        //[Fact]
        public async Task Connect()
        {
            var options = new SqlDbProviderOptions
            {
                ConnectionString = "Server=tcp:localhost,1433;Database=WebApp;Persist Security Info=False;User ID=sa;Password=Sa123456#;MultipleActiveResultSets=True;TrustServerCertificate=False;Connection Timeout=30;"
            };

            var provider = new SqlDbProvider(
                optionsAccessor: Options.Create<SqlDbProviderOptions>(options),
                logger: NullLogger<SqlDbProvider>.Instance
            );

            await provider.ConnectAsync();
            provider.Dispose();
            //Assert.NotNull(version);
        }

        //[Fact]
        public async Task ExecuteSqlAsync()
        {
            var options = new SqlDbProviderOptions
            {
                ConnectionString = "Server=tcp:localhost,1433;Database=WebApp;Persist Security Info=False;User ID=sa;Password=Sa123456#;MultipleActiveResultSets=True;TrustServerCertificate=False;Connection Timeout=30;"
            };

            var provider = new SqlDbProvider(
                optionsAccessor: Options.Create<SqlDbProviderOptions>(options),
                logger: NullLogger<SqlDbProvider>.Instance
            );

            await provider.ExecuteSqlAsync(@"
CREATE TABLE TestX (
	Id int NOT NULL,
	TimeUTC datetime NOT NULL,
    Name nvarchar(100) NULL,
	CONSTRAINT [PK_TestX] PRIMARY KEY (Id)
)
");

            //Assert.Null(version);
        }
    }
}