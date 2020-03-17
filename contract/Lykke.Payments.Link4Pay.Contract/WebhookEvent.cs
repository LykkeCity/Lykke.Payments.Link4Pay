using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Payments.Link4Pay.Contract
{
    public class WebhookEvent
    {
        public string WebhookId { get; set; }
        public string PaymentMode { get; set; }
        public int RetryOption { get; set; }
        public string MId { get; set; }
        public string Merchant { get; set; }
        public string Language { get; set; }
        public string NotificationType { get; set; }
        public string TransactionDate { get; set; }
        public int DeliveryAttempts { get; set; }
        public string Url { get; set; }
        public string TxnReference { get; set; }
        public string Provider { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionStatus OriginalTxnStatus { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusCode OriginalTxnStatusCode { get; set; }
        public string RespMsg { get; set; }
        public string CurrencyCode { get; set; }
        public string FirstAttemptDate { get; set; }
        public string TxnAmount { get; set; }
        public int RespCode { get; set; }
        public string Status { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusCode StatusCode { get; set; }
    }
}
