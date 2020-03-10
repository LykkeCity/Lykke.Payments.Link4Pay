using Common;
using Lykke.Payments.Link4Pay.Domain.Settings;
using Lykke.Payments.Link4Pay.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Payments.Link4Pay.Controllers
{
    [Route("redirect")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RedirectController : Controller
    {
        private readonly Link4PaySettings _link4PaySettings;

        public RedirectController(
            Link4PaySettings link4PaySettings
            )
        {
            _link4PaySettings = link4PaySettings;
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
    }
}
