using Newtonsoft.Json;
using ProtoBuf;

namespace Lykke.Payments.Link4Pay.Contract.Events
{
    [ProtoContract]
    public class CreditCardUsedEvent
    {
        [ProtoMember(1, IsRequired = true)]
        public string ClientId { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public string OrderId { get; set; }

        [ProtoMember(3, IsRequired = true)]
        public string CardHash { get; set; }

        [ProtoMember(4, IsRequired = true)]
        [JsonIgnore]
        public string CustomerName { get; set; }

        [ProtoMember(5, IsRequired = true)]
        public string CardNumber { get; set; }
    }
}
