using Lykke.SettingsReader.Attributes;

namespace Lykke.Payments.Link4Pay.Settings
{
    public class TelegramSettings
    {
        [Optional]
        public string Token { get; set; }
        [Optional]
        public string ChatId { get; set; }
    }
}
