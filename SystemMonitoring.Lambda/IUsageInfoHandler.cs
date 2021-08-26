using System.Threading.Tasks;

namespace SystemMonitoring.Lambda
{
    public interface IUsageInfoHandler
    {
        Task Store(UsageInfo body);
    }
}
