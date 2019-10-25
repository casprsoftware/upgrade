using System.Threading.Tasks;

namespace Upgrade
{
    /// <summary>
    /// SQL script source interface
    /// </summary>
    public interface ISqlScriptSource
    {
        /// <summary>
        /// Run the SQL script
        /// </summary>
        /// <param name="dbProvider">A database provider</param>
        /// <returns></returns>
        Task RunAsync(IDbProvider dbProvider);
    }
}