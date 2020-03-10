namespace Lykke.Payments.Link4Pay.Domain
{
    public class WebhookResponse
    {
        public string WebhookId { get; set; }
        public string MerchantId { get; set; }
        public string ModifiedDate { get; set; }
        public string Events { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public string MerchantName { get; set; }
    }
}
