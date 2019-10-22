using System;
using System.Threading.Tasks;

namespace Upgrade.Core.Test
{
    public class TestDbProvider : IDbProvider
    {
        public Task ConnectAsync()
        {
            return Task.CompletedTask;
        }

        public Task ExecuteSqlAsync(string sql)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}