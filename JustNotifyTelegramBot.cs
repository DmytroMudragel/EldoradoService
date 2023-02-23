using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EldoradoBot
{
    public class JustNotifyTelegramBot : ITelegramBot
    {
        private readonly object _Lock = new object();

        public static string? _TelegramBotToken { get; set; }

        public static string? _UserId { get; set; }

        private static ITelegramBotClient? _BotClient { get; set; }

        public bool? _IsTelegramBotActive { get; set; }

        public bool Init(string telegramBotToken,string userId)
        {
            try
            {
                _TelegramBotToken = telegramBotToken;
                _UserId = userId;
                if (_TelegramBotToken != "" && _UserId != "" && _TelegramBotToken is not null && _UserId is not null)
                {
                    _BotClient = new TelegramBotClient(_TelegramBotToken) { Timeout = TimeSpan.FromSeconds(20) };
                    var Me = _BotClient.GetMeAsync().Result;
                    if (Me != null && !string.IsNullOrEmpty(Me.FirstName))
                    {
                        _IsTelegramBotActive = true;
                        return true;
                    }
                    else
                    {
                        _IsTelegramBotActive = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Telegram bot failed to init", Logger.Status.EXEPTION, ex);
                _IsTelegramBotActive = false;
            }
            return false;
        }

        public bool SendPhoto(string text, InputOnlineFile inputOnlineFile)
        {
            lock (_Lock)
            {
                if (_UserId is not null)
                {
                    var res = _BotClient?.SendPhotoAsync(chatId: _UserId, inputOnlineFile, caption: text, replyMarkup: new ReplyKeyboardRemove()).Result;
                    if (res is not null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool SendMessage(string text)
        {
            lock (_Lock)
            {
                if (_UserId is not null)
                {
                    var res = _BotClient?.SendTextMessageAsync(chatId: _UserId, text: text, replyMarkup: new ReplyKeyboardRemove()).Result;
                    if (res is not null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
