using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.Domain
{
    public class CardPaymentResponse
    {
        public string Payload { get; set; }
        [JsonProperty("respCode")]
        public int ResponseCode { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
    }
}
