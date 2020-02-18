using Lykke.SettingsReader.Attributes;

namespace Lykke.Payments.Link4Pay.Contract
{
    public class Link4PayServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
        public string GrpcServiceUrl { get; set; }
    }
}
