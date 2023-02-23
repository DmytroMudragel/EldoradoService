using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;

namespace EldoradoBot
{
    public interface ITelegramBot
    {
        public bool? _IsTelegramBotActive { get; set; }
        bool Init(string telegramBotToken, string userId);
        bool SendPhoto(string text, InputOnlineFile inputOnlineFile, bool disableNotification);
        bool SendMessage(string text, bool disableNotification);
    }
}
