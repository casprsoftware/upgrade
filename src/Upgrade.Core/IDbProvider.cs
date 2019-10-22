using System;
using System.Threading.Tasks;

namespace Upgrade
{
    /// <summary>
    /// DB Provider Functions
    /// </summary>
    public interface IDbProvider : IDisposable
    {
        /// <summary>
        /// Connect to the Database
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// Execute SQL Script
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task ExecuteSqlAsync(string sql);
    }
}