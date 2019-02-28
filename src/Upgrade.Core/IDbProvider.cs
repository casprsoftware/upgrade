using System.Threading.Tasks;

namespace Upgrade
{
    /// <summary>
    /// DB Provider Functions
    /// </summary>
    public interface IDbProvider
    {
        /// <summary>
        /// Get Current Schema Version
        /// </summary>
        /// <returns></returns>
        Task<VersionInfo> GetSchemaVersionAsync();

        /// <summary>
        /// Set schema version
        /// </summary>
        /// <param name="version">The schema version</param>
        /// <returns></returns>
        Task SetSchemaVersionAsync(string version);

        /// <summary>
        /// Execute SQL Script
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task ExecuteSqlAsync(string sql);
    }
}