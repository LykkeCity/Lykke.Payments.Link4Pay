using System;

namespace Lykke.Payments.Link4Pay.Domain.Repositories.EventsLog
{
    public interface IPaymentTransactionLogEvent
    {
        string PaymentTransactrionId { get; }
        DateTime DateTime { get; }

        /// <summary>
        /// We have for shit cleaning processes
        /// </summary>
        string TechData { get; }

        /// <summary>
        /// We have for backoffice and other reports
        /// </summary>
        string Message { get; }

        string Who { get; }
    }
}
