using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using System.Windows.Forms;

namespace EldoradoBot
{
    public static class Logger                                                
    {
        private readonly static string _LogPath = Environment.CurrentDirectory + "\\Log.txt";   

        private static object _locker = new();

        public static ITelegramBot? _TelegramBot;

        public static void Init(ITelegramBot? telegramBot = null)
        {
            ClearLog();
            _TelegramBot = telegramBot;
            AddLogRecord($"Logger was successfully loaded", Status.OK);
            if (_TelegramBot != null)
            {
                if (_TelegramBot._IsTelegramBotActive is true)
                {
                    AddLogRecord($"Telegram bot was successfully loaded", Status.OK, true);
                }
                else
                {
                    AddLogRecord($"Telegram bot failed to init", Status.WARN);
                }
            }
        }

        public static void AddLogRecord(string text, Status status, bool telegram = false)
        {
            AddLogRecord(text, status,null, telegram);
        }

        public static void AddLogRecord(string text, Status status, Exception? exception, bool telegram = false)
        {
            string currentTimeString = $"[{DateTime.Now:HH:mm:ss}]";
            switch (status)
            {
                case Status.OK:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Status.WARN:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case Status.BAD:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Status.EXEPTION:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    break;
            }

            if (Console.GetCursorPosition().Top == 6000)
            {
                Console.Clear();
            }

            Console.WriteLine($"{currentTimeString} {text}");

            if (exception is not null)
                Console.WriteLine($"{currentTimeString} {exception.StackTrace}\n\n{exception.Message}");

            lock (_locker)
            {
                using (StreamWriter logFile = File.AppendText(_LogPath))
                {
                    logFile.WriteLine($"{currentTimeString} {text}");

                    if (exception is not null)
                        logFile.WriteLine($"{currentTimeString} {exception.StackTrace}\n\n{exception.Message}");
                }
            }

            if (_TelegramBot is not null && telegram)
            {
                if (_TelegramBot._IsTelegramBotActive is true && _TelegramBot.SendMessage($"[{DateTime.Now:HH:mm:ss}] {text}"))
                    Console.WriteLine($"{currentTimeString} Telegram message was successfully sended.");
                else
                    Console.WriteLine($"{currentTimeString} Error while sending telegram message.");
            }
        }

        public static void ClearLog()
        {
            if (File.Exists(_LogPath))
            {
                File.Delete(_LogPath);
            }   
        }

        public enum Status
        {
            OK = ConsoleColor.Green,
            WARN = ConsoleColor.Yellow,
            BAD = ConsoleColor.DarkRed,
            EXEPTION = ConsoleColor.Gray
        }
    }
}
