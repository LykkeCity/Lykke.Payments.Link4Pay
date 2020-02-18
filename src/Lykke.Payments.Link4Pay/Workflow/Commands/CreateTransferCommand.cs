using ProtoBuf;

namespace Lykke.Payments.Link4Pay.Workflow.Commands
{
    [ProtoContract]
    public class CreateTransferCommand
    {        
        [ProtoMember(1)]
        public string TransferId { get; set; }
        [ProtoMember(2)]
        public string ClientId { get; set; }
        [ProtoMember(3)]
        public string SourceClientId { get; set; }
        [ProtoMember(4)]
        public string AssetId { get; set; }
        [ProtoMember(5)]
        public double Amount { get; set; }
        [ProtoMember(6)]
        public string OrderId { get; set; }
        [ProtoMember(7)]
        public string WalletId { get; set; } 
    }
}