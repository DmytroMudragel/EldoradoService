using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace TF2Idle
{
    public static class Log
    {
        private readonly static string Path = Environment.CurrentDirectory + "\\log.txt";
        public static string? BotName { get; set; }
        public static Telegram? Telegram { get; set; }
        public static bool Init(string botname, string token, ulong uderid)
        {
            BotName = botname;
            Telegram = new Telegram(token, uderid);
            if (Telegram.Test())
                return true;
            else
                return false;
        }
        public static void Clear()
        {
            if (File.Exists(Path))
                File.Delete(Path);
        }
        public static void Append(Level level, string text, bool telegram = false)
        {
            Append(level, text, null, telegram);
        }
        public static void Append(Level level, string text, Exception? exception, bool telegram = false)
        {
            switch (level)
            {
                case Level.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case Level.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case Level.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case Level.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{BotName}] {text}");

            if (exception is not null)
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{BotName}] {exception.StackTrace}\n\n{exception.Message}");

            using (StreamWriter tw = File.AppendText(Path))
            {
                tw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{BotName}] {text}");

                if (exception is not null)
                    tw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{BotName}] {exception.StackTrace}\n\n{exception.Message}");
            }

            if (telegram)
            {
                if (Telegram is not null && BotName is not null && Telegram.Send($"[{DateTime.Now:HH:mm:ss}] [{BotName}] {text}"))
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{BotName}] Telegram message was successfully sended.");
                else
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{BotName}] Error while sending telegram message.");
            }
        }
        public enum Level
        {
            Debug,
            Info,
            Warning,
            Error
        }
    }

    public class Telegram
    {
        public readonly string Token;
        public readonly ulong UserId;
        public readonly string ApiEndpoint;

        private readonly HttpClient client = new();
        public Telegram(string token, ulong userid)
        {
            Token = token;
            UserId = userid;
            ApiEndpoint = $"https://api.telegram.org/bot{token}";
        }
        public bool Test()
        {
            return Task.Run(
                () => client.GetStringAsync($"{ApiEndpoint}/getMe").Result.Contains("\"ok\":true")
                ).ContinueWith((o) =>
                {
                    return !o.IsFaulted && o.Result;
                }).Result;
        }
        public bool Send(string text)
        {
            return Task.Run(() =>
            {
                SendMessageRequest request = new()
                {
                    ChatId = UserId.ToString(),
                    Text = text
                };

                var response = client.PostAsJsonAsync($"{ApiEndpoint}/sendMessage", request).Result;

                if (response.IsSuccessStatusCode)
                    return true;
                else
                    return false;
            }).ContinueWith((o) =>
            {
                return !o.IsFaulted && o.Result;
            }).Result;
        }

        public class SendMessageRequest
        {
            [JsonInclude]
            [JsonPropertyName("chat_id")]
            public string? ChatId { get; set; }
            [JsonInclude]
            [JsonPropertyName("text")]
            public string? Text { get; set; }
            [JsonInclude]
            [JsonPropertyName("parse_mode")]
            public string ParseMode { get; set; } = "Markdown";
        }
    }
}
