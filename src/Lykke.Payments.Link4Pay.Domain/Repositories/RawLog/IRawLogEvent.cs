using System;

namespace Lykke.Payments.Link4Pay.Domain.Repositories.RawLog
{
    public interface IRawLogEvent
    {
        DateTime DateTime { get; }
        string PaymentSystem { get; }
        string EventType { get; }
        string Data { get; }
    }
}
