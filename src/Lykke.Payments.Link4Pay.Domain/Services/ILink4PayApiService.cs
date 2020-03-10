using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Payments.Link4Pay.Domain.Services
{
    public interface ILink4PayApiService
    {
        Task<string> GetPaymentUrlAsync(CardPaymentRequest request);
        Task<WebhookResponse> CreateWebHookAsync(string url, List<string> events);
        Task<WebhookResponse[]> GetWebHookAsync(string webhookId);
        Task<TransactionStatusResponse> GetTransactionInfoAsync(string transactionId);
    }
}
