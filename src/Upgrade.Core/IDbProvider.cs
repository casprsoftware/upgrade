using System;
using System.Threading.Tasks;

namespace Upgrade
{
    /// <summary>
    /// Database provider interface
    /// </summary>
    public interface IDbProvider : IDisposable
    {
        /// <summary>
        /// Connect to the database
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// Execute SQL script
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task ExecuteSqlAsync(string sql);
    }
}