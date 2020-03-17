using Lykke.Payments.Link4Pay.Domain.Settings;

namespace ConsoleTools
{
    public class AppSettings
    {
        public string ClientPersonalInfoConnString { get; set; }
        public KeyVaultSettings KeyVault { get; set; }
        public Link4PaySettings Link4Pay { get; set; }
    }
}
