using System;
using System.Threading.Tasks;
using Lykke.Contracts.Payments;

namespace Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions
{
    public interface IPaymentTransactionsRepository
    {
        Task CreateAsync(IPaymentTransaction paymentTransaction);
        Task<IPaymentTransaction> GetByTransactionIdAsync(string id);

        /// <summary>
        /// Change transaction to process state and check if it's already processed or started being processed
        /// </summary>
        /// <param name="id">Id of transaction</param>
        /// <param name="paymentAggregatorTransactionId">Id of payment aggregator to update if transaction can be processed</param>
        /// <returns>Null - transaction is not exists or can not be processed</returns>
        Task<IPaymentTransaction> StartProcessingTransactionAsync(string id, string paymentAggregatorTransactionId = null);

        Task<IPaymentTransaction> SetStatusAsync(string id, PaymentStatus status);

        Task<IPaymentTransaction> SetAsOkAsync(string id, double depositedAmount, double? rate);

        Task<IPaymentTransaction> SetAntiFraudStatusAsync(string id, string antiFraudStatus);

        Task<bool> HasProcessedTransactionsAsync(string clientId, DateTime till);

        Task<IPaymentTransaction> SaveCardHashAsync(string id, string cardHash);
    }
}
