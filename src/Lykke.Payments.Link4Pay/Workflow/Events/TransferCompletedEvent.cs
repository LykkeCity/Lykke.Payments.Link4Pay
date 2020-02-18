using ProtoBuf;

namespace Lykke.Payments.Link4Pay.Workflow.Events
{
    [ProtoContract]
    public class TransferCompletedEvent
    {
        [ProtoMember(1)]
        public string OrderId { get; set; }
    }
}