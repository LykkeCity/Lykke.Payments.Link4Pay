using System;
using Lykke.Contracts.Payments;

namespace Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions
{
    public interface IPaymentTransaction
    {
        string Id { get; }
        string TransactionId { get; set; }
        string ClientId { get; }
        double Amount { get; }
        string AssetId { get; }
        string WalletId { get; }
        double? DepositedAmount { get; }
        string DepositedAssetId { get; }
        double? Rate { get; }
        string AggregatorTransactionId { get; }
        DateTime Created { get; }
        PaymentStatus Status { get; }
        CashInPaymentSystem PaymentSystem { get; }
        string Info { get; }
        string MeTransactionId { get; set; }
        string AntiFraudStatus { get; set; }
        string CardHash { get; set; }
        double FeeAmount { get; }
    }
}
