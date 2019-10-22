using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Upgrade.DbProviders
{
    /// <summary>
    /// MS SQL DB Provider
    /// </summary>
    public class SqlDbProvider : IDbProvider
    {
        #region Private Declarations

        private ILogger<SqlDbProvider> _logger;
        private readonly SqlDbProviderOptions _options;
        private IDbConnection _connection;

        #endregion

        #region Constructor

        public SqlDbProvider(
            IOptions<SqlDbProviderOptions> optionsAccessor, 
            ILogger<SqlDbProvider> logger)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
        }

        #endregion

        #region IDbProvider's Methods

        public Task ConnectAsync()
        {
            _connection = new SqlConnection(_options.ConnectionString);
            _connection.Open();
            return Task.CompletedTask;
        }

        public Task ExecuteSqlAsync(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(sql));
            }

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                cmd.ExecuteNonQuery();                
            }
            return Task.CompletedTask;         
        }

        #endregion

        public void Dispose()
        {
            _connection?.Close();
        }
    }
}