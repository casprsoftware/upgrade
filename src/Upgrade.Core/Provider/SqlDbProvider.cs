﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Upgrade.Provider
{
    public class SqlDbProvider : IDbProvider
    {
        #region Private Declarations

        private UpgradeOptions _options;

        #endregion

        #region Constructor

        public SqlDbProvider(IOptions<UpgradeOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }

        #endregion

        #region IDbProvider's Methods

        public Task<VersionInfo> GetSchemaVersionAsync()
        {
            
            //throw new NotImplementedException();
            return Task.FromResult<VersionInfo>(null);
        }

        public Task SetSchemaVersionAsync(int version)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task ExecuteSqlAsync(string sql)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;         
        }

        #endregion
    }
}