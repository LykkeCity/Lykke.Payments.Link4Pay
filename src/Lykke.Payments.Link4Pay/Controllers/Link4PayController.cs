using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Payments.Link4Pay.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Payments.Link4Pay.Controllers
{
    [Route("/link4pay")]
    public class Link4PayController : Controller
    {
        private readonly ILink4PayApiService _link4PayApiService;

        public Link4PayController(ILink4PayApiService link4PayApiService
        )
        {
            _link4PayApiService = link4PayApiService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> CreateWebhook(string url)
        {
            var result = await _link4PayApiService.CreateWebHookAsync(url, new List<string> {"payment", "void", "expired", "cancel"});
            return Ok(result);
        }

        [HttpGet("webhook")]
        public async Task<IActionResult> GetWebhook(string webhookId)
        {
            var result = await _link4PayApiService.GetWebHookAsync(webhookId);
            return Ok(result);
        }

        [HttpGet("transaction/{transactionId}")]
        public async Task<IActionResult> TransactionInfo(string transactionId)
        {
            var data = await _link4PayApiService.GetTransactionInfoAsync(transactionId);
            return Ok(data);
        }
    }
}
