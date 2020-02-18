using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.Domain
{
    public class CustomerInfo
    {
        [JsonProperty("billingAddress")]
        public AddressInfo BillingAddress { get; set; }
    }
}
