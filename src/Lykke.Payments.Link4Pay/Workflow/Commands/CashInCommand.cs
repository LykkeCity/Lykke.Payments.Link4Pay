using ProtoBuf;

namespace Lykke.Payments.Link4Pay.Workflow.Commands
{
    [ProtoContract]
    public class CashInCommand
    {
        [ProtoMember(1)]
        public string TransactionId { get; set; }

        [ProtoMember(2)]
        public string Request { get; set; }
    }
}
