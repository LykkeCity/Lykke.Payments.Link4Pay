using Lykke.SettingsReader.Attributes;

namespace Lykke.Payments.Link4Pay.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
        [AzureTableCheck]
        public string ClientPersonalInfoConnString { get; set; }
    }
}
