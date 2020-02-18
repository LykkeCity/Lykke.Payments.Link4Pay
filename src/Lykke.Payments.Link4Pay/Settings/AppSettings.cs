using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.PersonalData.Settings;

namespace Lykke.Payments.Link4Pay.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public Link4PayServiceSettings Link4PayService { get; set; }
        public PersonalDataServiceClientSettings PersonalDataServiceClient { get; set; }
        public ClientAccountServiceClientSettings ClientAccountServiceClient { get; set; }
        public FeeCalculatorServiceClientSettings FeeCalculatorServiceClient { get; set; }
        public ExchangeOperationsServiceClientSettings ExchangeOperationsServiceClient { get; set; }
    }
}
