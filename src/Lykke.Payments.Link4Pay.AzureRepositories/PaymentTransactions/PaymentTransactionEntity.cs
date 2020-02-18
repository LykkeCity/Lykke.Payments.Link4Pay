using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Contracts.Payments;
using Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions;

namespace Lykke.Payments.Link4Pay.AzureRepositories.PaymentTransactions
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    public class PaymentTransactionEntity : AzureTableEntity, IPaymentTransaction
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        string IPaymentTransaction.Id => Id.ToString();
        public string ClientId { get; set; }
        public DateTime Created { get; set; }
        public PaymentStatus Status { get; set; }
        public CashInPaymentSystem PaymentSystem { get; set; }
        public string Info { get; set; }
        public double? Rate { get; set; }
        public string AggregatorTransactionId { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string WalletId { get; set; }
        public double? DepositedAmount { get; set; }
        public string DepositedAssetId { get; set; }
        public string MeTransactionId { get; set; }
        public string AntiFraudStatus { get; set; }
        public string CardHash { get; set; }
        public double FeeAmount { get; set; }

        public static PaymentTransactionEntity Create(IPaymentTransaction src)
        {
            return new PaymentTransactionEntity
            {
                Created = src.Created,
                TransactionId = src.Id,
                Info = src.Info,
                ClientId = src.ClientId,
                AssetId = src.AssetId,
                WalletId = src.WalletId,
                Amount = src.Amount,
                FeeAmount = src.FeeAmount,
                AggregatorTransactionId = src.AggregatorTransactionId,
                DepositedAssetId = src.DepositedAssetId,
                Status = src.Status,
                PaymentSystem = src.PaymentSystem
            };
        }

        public static class IndexCommon
        {
            public static string GeneratePartitionKey() => "BCO";
        }

        public static class IndexByClient
        {
            public static string GeneratePartitionKey(string clientId) => clientId;
            public static string GenerateRowKey(string orderId) => orderId;
        }
    }
}
