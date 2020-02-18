using System;
using System.Threading.Tasks;
using Common;
using Lykke.Payments.Link4Pay.Domain;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.Domain.Settings;
using Lykke.Payments.Link4Pay.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Payments.Link4Pay.Controllers
{
    [Route("redirect")]
    public class RedirectController : Controller
    {
        private readonly Link4PaySettings _link4PaySettings;
        private readonly ILink4PayApiService _link4PayApiService;

        public RedirectController(
            Link4PaySettings link4PaySettings,
            ILink4PayApiService link4PayApiService
            )
        {
            _link4PaySettings = link4PaySettings;
            _link4PayApiService = link4PayApiService;
        }

        [HttpGet]
        public IActionResult RedirectUrl(string payload, string url)
        {
            var model = new RedirectFormModel
            {
                Payload = payload, Url = url.Base64ToString(), ClientId = _link4PaySettings.ClientId
            };

            return View(model);
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            string url = await _link4PayApiService.GetPaymentUrlAsync(new CardPaymentRequest
            {
                Merchant = new MerchantInfo {CustomerId = "1", MerchantId = _link4PaySettings.ClientId},
                Transaction =
                    new TransactionInfo
                    {
                        TxnAmount = 100.ToString("F2"),
                        CurrencyCode = "EUR",
                        TxnReference = Guid.NewGuid().ToString(),
                        Payout = false
                    },
                Url = new UrlInfo
                {
                    SuccessUrl = "http://link4pay-lykke.ngrok.io/ok",
                    FailUrl = "http://link4pay-lykke.ngrok.io/fail",
                    CancelUrl = "http://link4pay-lykke.ngrok.io/cancel"
                }
            });

            return Ok(url);
        }
    }
}
