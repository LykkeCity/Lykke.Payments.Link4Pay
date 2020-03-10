using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Payments.Link4Pay.Domain.Settings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Payments.Link4Pay.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Link4PayServiceSettings
    {
        public DbSettings Db { get; set; }
        public KeyVaultSettings KeyVault { get; set; }
        public Link4PaySettings Link4Pay { get; set; }
        public CqrsSettings Cqrs { get; set; }
        public int GrpcPort { get; set; }
        public string BankCardFeeClientId { get; set; }
        [Optional]
        public DateTime AntiFraudCheckRegistrationDateSince { get; set; } = new DateTime(2018, 7, 1);
        [Optional]
        public TimeSpan AntiFraudCheckPaymentPeriod { get; set; } = TimeSpan.FromDays(7);
        [Optional]
        public string AntiFraudNotificationEmail { get; set; }
        public string SourceClientId { get; set; }
        public IReadOnlyList<string> SupportedCountries { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> SupportedCurrencies { get; set; } = Array.Empty<string>();
    }
}
