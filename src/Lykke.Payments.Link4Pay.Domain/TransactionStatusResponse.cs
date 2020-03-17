using Common;
using Lykke.Payments.Link4Pay.Contract;
using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.Domain
{
    public class BinDetails
    {
        [JsonProperty("sub_brand_id")]
        public string SubBrrandId { get; set; }
        [JsonProperty("bin_family")]
        public string BinFamily { get; set; }
        public string Bin { get; set; }
        [JsonProperty("card_length")]
        public string CardLength { get; set; }
        public string Latitude { get; set; }
        [JsonProperty("card_type")]
        public string CardType { get; set; }
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        public string Bank { get; set; }
        [JsonProperty("commercial_level")]
        public int CommercialLevel { get; set; }
        [JsonProperty("country_name")]
        public string CountryName { get; set; }
        [JsonProperty("end_bin")]
        public string EndBin { get; set; }
        public string Brand { get; set; }
        [JsonProperty("commercial_level_type")]
        public string CommercialLevelType { get; set; }
        public string Longitude { get; set; }
    }

    public class CardDetails
    {
        public string TokenId { get; set; }
        public string CardHolderName { get; set; }
        public int Bin { get; set; }
        public string CvvStatus { get; set; }
        public string ThreeDSecure { get; set; }
        public string IssuerCountryCode { get; set; }
        public string CardType { get; set; }
        public string AvsStatus { get; set; }
        public string CardNo { get; set; }
        public string ExpiryDate { get; set; }
        public string IssuerCountry { get; set; }
        public string Last4digits { get; set; }
        public string CvvProvided { get; set; }
        public BinDetails BinDetails { get; set; }
        public string Region { get; set; }

        public string CardHash => $"{Bin}{CardType}{Last4digits}{ExpiryDate}".ToBase64();
    }

    public class TransactionStatusResponse
    {
        public string ReconciliationDate { get; set; }
        public string PaymentMode { get; set; }
        public int RetryOption { get; set; }
        public string Mid { get; set; }
        public string Merchant { get; set; }
        public string TxnType { get; set; }
        public string Language { get; set; }
        public string SettlementDate { get; set; }
        public string TransactionDate { get; set; }
        public string TxnReference { get; set; }
        public string SettlementCurrency { get; set; }
        public string ReconciliationStatus { get; set; }
        public string SettlementStatus { get; set; }
        public string Provider { get; set; }
        public TransactionStatus OriginalTxnStatus { get; set; }
        public int OriginalTxnStatusCode { get; set; }
        public string RespMsg { get; set; }
        public string SettlementAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string TxnAmount { get; set; }
        public int RespCode { get; set; }
        public CardDetails Card { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }
    }
}
