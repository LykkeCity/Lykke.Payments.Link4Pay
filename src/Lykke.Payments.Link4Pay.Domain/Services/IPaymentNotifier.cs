using System.Threading.Tasks;
using Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions;

namespace Lykke.Payments.Link4Pay.Domain.Services
{
    public interface IPaymentNotifier
    {
        Task NotifyAsync(IPaymentTransaction paymentTransaction);
    }
}
