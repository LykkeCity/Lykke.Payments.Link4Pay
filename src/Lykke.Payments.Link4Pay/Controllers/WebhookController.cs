using System;
using System.IO;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Payments.Link4Pay.Contract;
using Lykke.Payments.Link4Pay.Domain;
using Lykke.Payments.Link4Pay.Domain.Repositories.RawLog;
using Lykke.Payments.Link4Pay.Domain.Services;
using Lykke.Payments.Link4Pay.Workflow.Commands;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lykke.Payments.Link4Pay.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WebhookController : Controller
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IRawLogRepository _rawLogRepository;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ILog _log;

        public WebhookController(
            IEncryptionService encryptionService,
            IRawLogRepository rawLogRepository,
            ICqrsEngine cqrsEngine,
            ILogFactory logFactory
            )
        {
            _encryptionService = encryptionService;
            _rawLogRepository = rawLogRepository;
            _cqrsEngine = cqrsEngine;
            _log = logFactory.CreateLog(this);
        }

        [HttpPost]
        [Route("/webhook")]
        public async Task<ActionResult> Webhook()
        {
            _log.Info("Webhook");
            try
            {
                WebhookEvent webhookEvent;
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    var data = _encryptionService.Decrypt(body);
                    webhookEvent = JsonConvert.DeserializeObject<WebhookEvent>(data);

                    if (webhookEvent?.OriginalTxnStatus == TransactionStatus.Pending)
                    {
                        _log.Info("Skip webhook event with pending status");
                        return Ok();
                    }
                }

                if (webhookEvent != null)
                {
                    string data = webhookEvent.ToJson();
                    _log.Info($"Webhook data {webhookEvent.TxnReference}", context: data);
                    await _rawLogRepository.RegisterEventAsync(RawLogEvent.Create(nameof(Webhook), data));

                     _cqrsEngine.SendCommand(new CashInCommand
                         {
                             TransactionId = webhookEvent.TxnReference,
                             Request = data
                         },
                         Link4PayBoundedContext.Name, Link4PayBoundedContext.Name);
                }
            }
            catch(Exception ex)
            {
                _log.Error(ex);
                return Ok();
            }

            return Ok();
        }
    }
}
