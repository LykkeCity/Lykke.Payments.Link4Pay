using System.Threading.Tasks;

namespace Lykke.Payments.Link4Pay.Domain.Repositories.RawLog
{
    public interface IRawLogRepository
    {
        Task RegisterEventAsync(IRawLogEvent evnt, string clientId = null);
    }
}
