using System;

namespace Lykke.Payments.Link4Pay.Domain.Repositories.EventsLog
{
    public interface IPaymentTransactionLogEvent
    {
        string PaymentTransactrionId { get; }
        DateTime DateTime { get; }
        string TechData { get; }
        string Message { get; }
        string Who { get; }
    }
}
