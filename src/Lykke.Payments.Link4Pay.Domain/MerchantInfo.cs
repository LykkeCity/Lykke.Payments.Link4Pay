using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.Domain
{
    public class MerchantInfo
    {
        [JsonProperty("merchantID")]
        public string MerchantId { get; set; }
        [JsonProperty("customerID")]
        public string CustomerId { get; set; }
    }
}
