using Autofac;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Payments.Link4Pay.AzureRepositories.EventsLog;
using Lykke.Payments.Link4Pay.AzureRepositories.PaymentTransactions;
using Lykke.Payments.Link4Pay.AzureRepositories.RawLog;
using Lykke.Payments.Link4Pay.Domain.Repositories.EventsLog;
using Lykke.Payments.Link4Pay.Domain.Repositories.PaymentTransactions;
using Lykke.Payments.Link4Pay.Domain.Repositories.RawLog;
using Lykke.Payments.Link4Pay.Settings;
using Lykke.SettingsReader;

namespace Lykke.Payments.Link4Pay.Modules
{
    [UsedImplicitly]
    public class RepositoriesModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public RepositoriesModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(ctx =>
                new RawLogRepository(AzureTableStorage<RawLogEventEntity>.Create(
                    _appSettings.ConnectionString(x => x.Link4PayService.Db.LogsConnString), "PaymentSystemsLog", ctx.Resolve<ILogFactory>()))
            ).As<IRawLogRepository>().SingleInstance();

            builder.Register(ctx =>
                new PaymentTransactionsRepository(
                    AzureTableStorage<PaymentTransactionEntity>.Create(
                        _appSettings.ConnectionString(i => i.Link4PayService.Db.ClientPersonalInfoConnString),
                        "PaymentTransactions", ctx.Resolve<ILogFactory>()),
                    AzureTableStorage<AzureMultiIndex>.Create(
                        _appSettings.ConnectionString(i => i.Link4PayService.Db.ClientPersonalInfoConnString),
                        "PaymentTransactions", ctx.Resolve<ILogFactory>())
                )
            ).As<IPaymentTransactionsRepository>().SingleInstance();

            builder.Register(ctx =>
                new PaymentTransactionEventsLog(AzureTableStorage<PaymentTransactionLogEventEntity>.Create(
                    _appSettings.ConnectionString(i => i.Link4PayService.Db.LogsConnString), "PaymentsLog", ctx.Resolve<ILogFactory>()))
            ).As<IPaymentTransactionEventsLog>().SingleInstance();
        }
    }
}
