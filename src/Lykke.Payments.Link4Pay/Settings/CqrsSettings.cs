using Lykke.SettingsReader.Attributes;

namespace Lykke.Payments.Link4Pay.Settings
{
    public class CqrsSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
    }
}
