using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Payments.Link4Pay.Domain.Repositories.RawLog;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Payments.Link4Pay.Controllers
{
    public class CallbacksController : Controller
    {
        private readonly IRawLogRepository _rawLogRepository;
        private readonly ILog _log;

        public CallbacksController(
            IRawLogRepository rawLogRepository,
            ILogFactory logFactory
            )
        {
            _rawLogRepository = rawLogRepository;
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [HttpPost]
        [Route("/ok")]
        public async Task<ActionResult> OkPage()
        {
            _log.Info($"Ok page ({Request.QueryString.ToString()})");
            await _rawLogRepository.RegisterEventAsync(RawLogEvent.Create("Ok page", Request.QueryString.ToString()));
            return View();
        }

        [HttpGet]
        [HttpPost]
        [Route("/fail")]
        public async Task<ActionResult> FailPage()
        {
            _log.Info($"Fail page ({Request.QueryString.ToString()})");
            await _rawLogRepository.RegisterEventAsync(RawLogEvent.Create("Fail page", Request.QueryString.ToString()));
            return View();
        }

        [HttpGet]
        [HttpPost]
        [Route("/cancel")]
        public async Task<ActionResult> CancelPage()
        {
            _log.Info($"Cancel page ({Request.QueryString.ToString()})");
            await _rawLogRepository.RegisterEventAsync(RawLogEvent.Create("Cancel page", Request.QueryString.ToString()));
            return View();
        }
    }
}
