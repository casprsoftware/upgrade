using System.Threading.Tasks;

namespace Upgrade
{
    public interface ISqlRunner
    {
        Task RunAsync(IDbProvider dbProvider);
    }
}