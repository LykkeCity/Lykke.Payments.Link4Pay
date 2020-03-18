using ProtoBuf;

namespace ConsoleTools
{
    [ProtoContract]
    public class ProcessingStartedEvent
    {
        [ProtoMember(1)]
        public string OrderId { get; set; }
    }
}
