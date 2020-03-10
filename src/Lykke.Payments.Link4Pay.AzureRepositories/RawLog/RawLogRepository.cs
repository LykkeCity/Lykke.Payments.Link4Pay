using System.Threading.Tasks;
using AzureStorage;
using Lykke.Payments.Link4Pay.Domain.Repositories.RawLog;

namespace Lykke.Payments.Link4Pay.AzureRepositories.RawLog
{
    public class RawLogRepository : IRawLogRepository
    {
        private readonly INoSQLTableStorage<RawLogEventEntity> _tableStorage;

        public RawLogRepository(INoSQLTableStorage<RawLogEventEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task RegisterEventAsync(IRawLogEvent evnt, string clientId = null)
        {
            var newEntity = RawLogEventEntity.Create(evnt);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, evnt.DateTime);
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var byCLient = RawLogEventEntity.CreateByClient(evnt, clientId);
                await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(byCLient, evnt.DateTime);
            }
        }
    }
}
