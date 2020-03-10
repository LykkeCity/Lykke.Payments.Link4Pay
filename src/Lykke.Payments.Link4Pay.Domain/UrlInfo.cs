using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.Domain
{
    public class UrlInfo
    {
        [JsonProperty("successURL")]
        public string SuccessUrl { get; set; }
        [JsonProperty("failURL")]
        public string FailUrl { get; set; }
        [JsonProperty("cancelURL")]
        public string CancelUrl { get; set; }
        [JsonProperty("showConfirmationPage")]
        public bool ShowConfirmationPage { get; set; }
        [JsonProperty("cartURL")]
        public string CartUrl { get; set; }
        [JsonProperty("productURL")]
        public string ProductUrl { get; set; }
        [JsonProperty("iFrame")]
        public bool Iframe { get; set; }
    }
}
