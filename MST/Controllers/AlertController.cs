// Controllers/AlertController.cs
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using MST.Models;

namespace MST.Controllers
{
    public class AlertController : Controller
    {
        private readonly ITelegramBotClient _botClient;
        private readonly string _groupChatId = "-1002893337685";

        public AlertController(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        [HttpGet]
        public IActionResult SendAlert()
        {
            return View(new AlertMessage());
        }

        [HttpPost]
        public async Task<IActionResult> SendAlert(AlertMessage model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Send message to the Telegram group
                object value = await _botClient.SendTextMessageAsync(
                    chatId: _groupChatId,
                    text: model.Description,
                    disableNotification: false 
                );

                TempData["Message"] = "Alert sent successfully!";
                return RedirectToAction("SendAlert");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to send alert: {ex.Message}";
                return View(model);
            }
        }
    }
}