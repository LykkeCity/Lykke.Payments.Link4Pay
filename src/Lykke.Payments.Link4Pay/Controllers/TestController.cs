using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Payments.Link4Pay.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Payments.Link4Pay.Controllers
{
    public class TestController : Controller
    {
        private readonly ILink4PayApiService _link4PayApiService;

        public TestController(ILink4PayApiService link4PayApiService
            )
        {
            _link4PayApiService = link4PayApiService;
        }

        [HttpGet("/test")]
        public async Task<IActionResult> Test()
        {
            await _link4PayApiService.CreateWebHookAsync("https://payments-link4pay-dev.lykkex.net/webhook",
                new List<string> {"payment", "void", "expired", "cancel"});
            return Ok();
        }

        [HttpGet("/transactionInfo/{transactionId}")]
        public async Task<IActionResult> TransactionInfo(string transactionId)
        {
            var data = await _link4PayApiService.GetTransactionInfoAsync(transactionId);
            return Ok(data);
        }
    }
}
