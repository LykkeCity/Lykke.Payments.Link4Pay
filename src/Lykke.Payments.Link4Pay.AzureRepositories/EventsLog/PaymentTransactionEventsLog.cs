using System.Threading.Tasks;
using AzureStorage;
using Lykke.Payments.Link4Pay.Domain.Repositories.EventsLog;

namespace Lykke.Payments.Link4Pay.AzureRepositories.EventsLog
{
    public class PaymentTransactionEventsLog : IPaymentTransactionEventsLog
    {
        private readonly INoSQLTableStorage<PaymentTransactionLogEventEntity> _tableStorage;

        public PaymentTransactionEventsLog(INoSQLTableStorage<PaymentTransactionLogEventEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task WriteAsync(IPaymentTransactionLogEvent newEvent)
        {
            var newEntity = PaymentTransactionLogEventEntity.Create(newEvent);
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, newEntity.DateTime);
        }
    }
}
