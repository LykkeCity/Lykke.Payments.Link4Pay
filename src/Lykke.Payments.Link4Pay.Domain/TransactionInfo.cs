using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.Domain
{
    public class TransactionInfo
    {
        [JsonProperty("txnAmount")]
        public string TxnAmount { get; set; }
        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }
        [JsonProperty("txnReference")]
        public string TxnReference { get; set; }
        [JsonProperty("payout")]
        public bool Payout { get; set; }
    }
}
