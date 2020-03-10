namespace Lykke.Payments.Link4Pay.Domain.Settings
{
    public class KeyVaultSettings
    {
        public string VaultBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CertificateName { get; set; }
        public string PasswordKey { get; set; }

    }
}
