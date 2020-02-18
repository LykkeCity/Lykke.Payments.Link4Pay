using System.Threading.Tasks;

namespace Lykke.Payments.Link4Pay.Domain.Repositories.EventsLog
{
    public interface IPaymentTransactionEventsLog
    {
        Task WriteAsync(IPaymentTransactionLogEvent newEvent);
    }
}
