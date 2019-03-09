using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Upgrade.Provider
{
    /// <summary>
    /// MS SQL DB Provider
    /// </summary>
    public class SqlDbProvider : IDbProvider
    {
        private const string TableName = "__Versions";

        #region Private Declarations

        private ILogger<SqlDbProvider> _logger;
        private readonly UpgradeOptions _options;
        private IDbConnection _connection;

        #endregion

        #region Constructor

        public SqlDbProvider(
            IOptions<UpgradeOptions> optionsAccessor, 
            ILogger<SqlDbProvider> logger)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
        }

        #endregion

        #region IDbProvider's Methods

        public Task<VersionInfo> GetSchemaVersionAsync()
        {
            _logger.LogDebug("Getting version info");
            var exists = ExistsTable();
            if (!exists)
            {
                return Task.FromResult<VersionInfo>(null);
            }

            var connection = GetOrCreateConnection();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;                
                cmd.CommandText = $@"
SELECT TOP 1 Id, TimeUTC
FROM {TableName}
ORDER BY TimeUTC DESC";
                VersionInfo versionInfo = null;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    versionInfo = new VersionInfo
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        TimeUTC = reader.GetDateTime(reader.GetOrdinal("TimeUTC"))
                    };       
                }
                return Task.FromResult<VersionInfo>(versionInfo);
            }
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

        #region Private Methods

        private IDbConnection GetOrCreateConnection()
        {
            if (_connection == null || _connection.State == ConnectionState.Closed)
            {
                _connection = new SqlConnection(_options.ConnectionString);
                _connection.Open();
            }

            return _connection;
        }

        private bool ExistsTable()
        {
            var exists = false;
            var connection = GetOrCreateConnection();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                
                var paramTableName = cmd.CreateParameter();
                paramTableName.ParameterName = "TableName";
                paramTableName.Value = TableName;
                cmd.Parameters.Add(paramTableName);

                cmd.CommandText = @"
SELECT COUNT(TABLE_NAME) AS TABLE_EXISTS 
FROM INFORMATION_SCHEMA.TABLES tbls
WHERE tbls.TABLE_NAME = @TableName";

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    exists = reader.GetInt32(reader.GetOrdinal("TABLE_EXISTS")) == 1;                   
                }                
            }
            return exists;
        }

        #endregion
    }
}