using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Upgrade
{
    /// <summary>
    /// Upgrade manager
    /// </summary>
    public class UpgradeManager
    {
        #region Private Declarations
        
        private readonly ILogger _logger;
        private readonly IDbProvider _dbProvider;
        private readonly ISqlScriptSource _sqlScriptSource;

        #endregion

        #region Constructor

        public UpgradeManager(
            ILoggerFactory loggerFactory, 
            IDbProvider dbProvider,
            ISqlScriptSource sqlScriptSource)
        {
            _dbProvider = dbProvider;
            _sqlScriptSource = sqlScriptSource;
            _logger = loggerFactory.CreateLogger(typeof(UpgradeManager));
        }

        #endregion

        #region Public Methods

        public async Task RunAsync()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("RUN START");
            }

            try
            {
                await _dbProvider.ConnectAsync();

                await _sqlScriptSource.RunAsync(_dbProvider);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured while executing upgrade db.");
            }
            finally
            {
                try
                {
                    _dbProvider.Dispose();
                }
                catch(Exception)
                {
                    _logger.LogWarning("Cannot dispose database provider.");
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("RUN FINISH");
                }
            }
        }

        #endregion
    }
}
