using System.Threading.Tasks;
using Lykke.Payments.Link4Pay.Settings;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;

namespace Lykke.Payments.Link4Pay.Controllers
{
    [Route("/telegram")]
    public class TelegramController : Controller
    {
        private readonly ITelegramBotClient _telegramBot;
        private readonly string _chatId;

        public TelegramController(
            ITelegramBotClient telegramBot,
            TelegramSettings telegramSettings
            )
        {
            _telegramBot = telegramBot;
            _chatId = telegramSettings.ChatId;
        }

        [HttpPost]
        [Route("/send")]
        public async Task<ActionResult> SendMessage([FromBody] string message)
        {
            await _telegramBot.SendTextMessageAsync(_chatId, message);
            return Ok();
        }
    }
}
