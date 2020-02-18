using System;
using Lykke.Payments.Link4Pay.Domain.Repositories.RawLog;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Payments.Link4Pay.AzureRepositories.RawLog
{
    public class RawLogEventEntity : TableEntity, IRawLogEvent
    {
        public DateTime DateTime { get; set; }
        public string PaymentSystem => PartitionKey;
        public string EventType { get; set; }
        public string Data { get; set; }

        public static string GeneratePartitionKey(string paymentSystem) => paymentSystem;

        public static RawLogEventEntity Create(IRawLogEvent src)
        {
            return new RawLogEventEntity
            {
                PartitionKey = GeneratePartitionKey(src.PaymentSystem),
                DateTime = src.DateTime,
                Data = src.Data,
                EventType = src.EventType
            };
        }

        public static RawLogEventEntity CreateByClient(IRawLogEvent src, string clientId)
        {
            return new RawLogEventEntity
            {
                PartitionKey = clientId,
                DateTime = src.DateTime,
                Data = src.Data,
                EventType = src.EventType
            };
        }
    }
}
