using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.Domain
{
    public class CardPaymentRequest
    {
        [JsonProperty("lang")]
        public string Lang { get; set; } = "en";
        [JsonProperty("merchant")]
        public MerchantInfo Merchant { get; set; }
        [JsonProperty("customer")]
        public CustomerInfo Customer { get; set; }
        [JsonProperty("transaction")]
        public TransactionInfo Transaction { get; set; }
        [JsonProperty("url")]
        public UrlInfo Url { get; set; }
    }
}
