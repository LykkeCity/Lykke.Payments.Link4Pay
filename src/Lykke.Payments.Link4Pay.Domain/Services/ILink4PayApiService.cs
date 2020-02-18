using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Payments.Link4Pay.Domain.Services
{
    public interface ILink4PayApiService
    {
        Task<string> GetPaymentUrlAsync(CardPaymentRequest request);
        Task CreateWebHookAsync(string url, List<string> events);
        Task<TransactionStatusResponse> GetTransactionInfoAsync(string transactionId);
    }
}
