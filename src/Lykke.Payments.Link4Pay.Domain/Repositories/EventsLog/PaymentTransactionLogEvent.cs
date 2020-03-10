using System;

namespace Lykke.Payments.Link4Pay.Domain.Repositories.EventsLog
{
    public class PaymentTransactionLogEvent : IPaymentTransactionLogEvent
    {
        public string PaymentTransactrionId { get; set; }
        public DateTime DateTime { get; set; }
        public string TechData { get; set; }
        public string Message { get; set; }
        public string Who { get; set; }

        public static PaymentTransactionLogEvent Create(
            string transactionId,
            string techData,
            string message,
            string who)
        {
            return new PaymentTransactionLogEvent
            {
                PaymentTransactrionId = transactionId,
                DateTime = DateTime.UtcNow,
                Message = message,
                TechData = techData,
                Who = who
            };
        }
    }
}
