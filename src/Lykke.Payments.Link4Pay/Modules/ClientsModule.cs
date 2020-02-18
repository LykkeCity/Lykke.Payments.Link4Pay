using Autofac;
using Lykke.Common.Log;
using Lykke.Messages.Email;
using Lykke.Payments.Link4Pay.Settings;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.SettingsReader;

namespace Lykke.Payments.Link4Pay.Modules
{
    public class ClientsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settingsManager;

        public ClientsModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settingsManager = settingsManager;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var appSettings = _settingsManager.CurrentValue;

            builder.RegisterFeeCalculatorClient(appSettings.FeeCalculatorServiceClient.ServiceUrl);

            builder.RegisterInstance(
                new ExchangeOperationsServiceClient(appSettings.ExchangeOperationsServiceClient.ServiceUrl)
            ).As<IExchangeOperationsServiceClient>()
            .SingleInstance();

            builder.RegisterClientAccountClient(appSettings.ClientAccountServiceClient.ServiceUrl);

            builder.Register(ctx => new PersonalDataService(appSettings.PersonalDataServiceClient, ctx.Resolve<ILogFactory>()))
                .As<IPersonalDataService>().SingleInstance();
            builder.Register(ctx => new CreditCardsService(appSettings.PersonalDataServiceClient, ctx.Resolve<ILogFactory>()))
                .As<ICreditCardsService>().SingleInstance();

            builder.RegisterEmailSenderViaAzureQueueMessageProducer(_settingsManager.ConnectionString(x => x.Link4PayService.Db.ClientPersonalInfoConnString));
        }
    }
}
