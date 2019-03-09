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
                _logger.LogDebug("Table '{TableName}' doesn't exist in database", TableName);
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
            if (version == 0)
            {
                throw new ArgumentException("Version must be great than zero.", nameof(version));
            }

            var exists = ExistsTable();
            if (!exists)
            {
                CreateTable();
            }

            var connection = GetOrCreateConnection();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;

                var paramId = cmd.CreateParameter();
                paramId.ParameterName = "Id";
                paramId.Value = version;
                cmd.Parameters.Add(paramId);

                var paramTime = cmd.CreateParameter();
                paramTime.ParameterName = "Time";
                paramTime.Value = DateTime.UtcNow;
                cmd.Parameters.Add(paramTime);

                cmd.CommandText = $"INSERT INTO {TableName} (Id, TimeUTC) VALUES (@Id, @Time)";

                var result = cmd.ExecuteNonQuery();
                if (result==0)
                {
                    throw new Exception("cannot set version");
                }
            }

            return Task.CompletedTask;
        }

        public Task ExecuteSqlAsync(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(sql));
            }

            var connection = GetOrCreateConnection();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                cmd.ExecuteNonQuery();                
            }
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

        private void CreateTable()
        {            
            var connection = GetOrCreateConnection();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;              
                cmd.CommandText = $@"
-- Table Versions
CREATE TABLE {TableName} (
	Id int NOT NULL,
	TimeUTC datetime NOT NULL,

	CONSTRAINT [PK_Versions] PRIMARY KEY (Id)
)

-- Index on column TimeUTC
CREATE INDEX IDX_Versions_TimeUTC
ON {TableName} (TimeUTC)
";

                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}