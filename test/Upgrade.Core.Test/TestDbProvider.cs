using System;
using System.Threading.Tasks;

namespace Upgrade.Core.Test
{
    public class TestDbProvider : IDbProvider
    {
        private VersionInfo _versionInfo;

        public TestDbProvider(VersionInfo versionInfo)
        {
            _versionInfo = versionInfo;
        }

        public TestDbProvider() : this(null)
        {            
        }

        public Task<VersionInfo> GetSchemaVersionAsync()
        {
            return Task.FromResult(_versionInfo);
        }

        public Task SetSchemaVersionAsync(int version)
        {
            if (_versionInfo == null)
            {
                _versionInfo = new VersionInfo();
            }
            _versionInfo.Id = version;
            _versionInfo.TimeUTC = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public Task ExecuteSqlAsync(string sql)
        {
            return Task.CompletedTask;
        }
    }
}