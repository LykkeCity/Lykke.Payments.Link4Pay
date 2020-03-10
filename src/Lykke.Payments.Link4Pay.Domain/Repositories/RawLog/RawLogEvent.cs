using System;
using Lykke.Contracts.Payments;

namespace Lykke.Payments.Link4Pay.Domain.Repositories.RawLog
{
    public class RawLogEvent : IRawLogEvent
    {
        public DateTime DateTime { get; set; }
        public string PaymentSystem { get; set; }
        public string EventType { get; set; }
        public string Data { get; set; }

        public static RawLogEvent Create(string eventType, string data)
        {
            return new RawLogEvent
            {
                DateTime = DateTime.UtcNow,
                PaymentSystem = CashInPaymentSystem.Link4Pay.ToString(),
                Data = data,
                EventType = eventType
            };
        }
    }
}
