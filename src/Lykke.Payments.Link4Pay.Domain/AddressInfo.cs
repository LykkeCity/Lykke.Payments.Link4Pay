using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.Domain
{
    public class AddressInfo
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [JsonProperty("mobileNo")]
        public string MobileNo { get; set; }
        [JsonProperty("emailId")]
        public string EmailId { get; set; }
    }
}
