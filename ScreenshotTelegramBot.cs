//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Drawing;
//using Telegram.Bot;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.InputFiles;
//using Telegram.Bot.Types.ReplyMarkups;
//using System.Windows.Forms;
//using System.Drawing.Imaging;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Runtime.InteropServices;

//namespace EldoradoBot
//{
//    public class ScreenshotTelegramBot : ITelegramBot
//    {
//        public static string? _TelegramBotToken { get; set; }

//        public static string? _UserId { get; set; }

//        //public static string? _BotName { get; set; }

//        private static ITelegramBotClient? _BotClient { get; set; }

//        public bool? _IsTelegramBotActive { get; set; }

//        private int _LastSendedMessage { get; set; }

//        private const string GetScreenshotBtnText = "Get Screenshot";

//        public bool Init(string telegramBotToken,string userId)
//        {
//            try
//            {
//                _TelegramBotToken = telegramBotToken;
//                _UserId = userId;
//                if (_TelegramBotToken != "" && _UserId != "" && _TelegramBotToken is not null && _UserId is not null)
//                {
//                    _BotClient = new TelegramBotClient(_TelegramBotToken) { Timeout = TimeSpan.FromSeconds(20) };
//                    var Me = _BotClient.GetMeAsync().Result;
//                    if (Me != null && !string.IsNullOrEmpty(Me.FirstName))
//                    {
//                        _IsTelegramBotActive = true;
//                        Thread telegramThread = new Thread(new ThreadStart(GetUpdates));
//                        telegramThread.Start();
//                        return true;
//                        //GetUpdates();
//                    }
//                    else
//                    {
//                        _IsTelegramBotActive = false;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.AddLogRecord($"Telegram bot failed to init", Logger.Status.EXEPTION, ex);
//                _IsTelegramBotActive = false;
//            }
//            return false;
//        }

//public void GetScreenshot()
//{
//    Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
//                            Screen.PrimaryScreen.Bounds.Height);
//    Graphics graphics = Graphics.FromImage(bitmap as Image);
//    graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
//    bitmap.Save(@"C:\Temp\printscreen" + Guid.NewGuid() + ".jpg", ImageFormat.Jpeg);
//}

//public Bitmap GetScreenShot()
//{
//    Rectangle screen = Screen.FromControl(this).Bounds;
//    Rectangle bounds = new Rectangle(0, 0, System.Windows.SystemParameters.PrimaryScreenWidth , (int)SystemParameters.VirtualScreenHeight);
//    Bitmap Screenshot = new Bitmap(bounds.Width, bounds.Height);
//    using (Graphics graphics = Graphics.FromImage(Screenshot))
//    {
//        graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
//    }
//    return Screenshot;
//}

//        public bool SendPhoto(string text, InputOnlineFile inputOnlineFile)
//        {
//            if (_UserId is not null)
//            {
//                var res = _BotClient?.SendPhotoAsync(chatId: _UserId, inputOnlineFile, caption: text, replyMarkup: Buttons()).Result;
//                if (res is not null)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }

//        public bool SendMessage(string text)
//        {
//            if (_UserId is not null)
//            {
//                var res = _BotClient?.SendTextMessageAsync(chatId: _UserId, text: text, replyMarkup: Buttons()).Result;
//                if (res is not null)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }

//        public void GetUpdates()
//        {
//            int offset = 0;
//            while (true)
//            {
//                try
//                {
//                    if (_BotClient != null)
//                    {
//                        var Updates = _BotClient.GetUpdatesAsync(offset).Result;
//                        if (Updates != null && Updates.Length > 0)
//                        {
//                            foreach (var update in Updates)
//                            {
//                                ProcessUpdate(update);
//                                offset = update.Id + 1;
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex) { Logger.AddLogRecord(ex.Message.ToString(), Logger.Status.EXEPTION, ex); }
//                Thread.Sleep(1000);
//            }
//        }

//        public void TakeScreenshot(string path)
//        {
//            try
//            {
//                InternalTakeScreenshot(path);
//            }
//            catch (Win32Exception)
//            {
//                var winDir = System.Environment.GetEnvironmentVariable("WINDIR");
//                Process.Start(
//                    Path.Combine(winDir, "system32", "tscon.exe"),
//                    String.Format("{0} /dest:console", GetTerminalServicesSessionId()))
//                .WaitForExit();

//                InternalTakeScreenshot(path);
//            }
//        }

//        public void InternalTakeScreenshot(string path)
//        {
//            var point = new System.Drawing.Point(0, 0);
//            var bounds = System.Windows.Forms.Screen.GetBounds(point);

//            var size = new System.Drawing.Size(bounds.Width, bounds.Height);
//            var screenshot = new System.Drawing.Bitmap(bounds.Width, bounds.Height);
//            var g = System.Drawing.Graphics.FromImage(screenshot);
//            g.CopyFromScreen(0, 0, 0, 0, size);

//            screenshot.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
//            using (var imageFile = File.OpenRead("tg.jpg"))
//            {
//                InputOnlineFile? inputOnlineFile = new InputOnlineFile(imageFile, "Screenshot.jpg");
//                if (_UserId is not null)
//                {
//                    SendPhoto("", inputOnlineFile);
//                }
//            }
//        }

//        [DllImport("kernel32.dll")]
//        static extern bool ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);

//        private static uint GetTerminalServicesSessionId()
//        {
//            var proc = Process.GetCurrentProcess();
//            var pid = proc.Id;

//            var sessionId = 0U;
//            if (ProcessIdToSessionId((uint)pid, out sessionId))
//                return sessionId;
//            return 1U; // fallback, the console session is session 1
//        }

//        //public void GetScreenshot()
//        //{
//        //    try
//        //    {
//        //        Rectangle bounds = new Rectangle(0, 0, (int)SystemInformation.VirtualScreen.Width, (int)SystemInformation.VirtualScreen.Height);
//        //        Bitmap Screenshot = new Bitmap(bounds.Width, bounds.Height);
//        //        using (Graphics graphics = Graphics.FromImage(Screenshot))
//        //        {
//        //            graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
//        //            using (MemoryStream memory = new MemoryStream())
//        //            {
//        //                using (FileStream fs = new FileStream("tg.jpg", FileMode.Create, FileAccess.ReadWrite))
//        //                {
//        //                    Screenshot.Save(memory, ImageFormat.Jpeg);
//        //                    byte[] bytes = memory.ToArray();
//        //                    fs.Write(bytes, 0, bytes.Length);
//        //                }
//        //            }
//        //        }
//        //        Screenshot.Dispose();
//        //        using (var imageFile = File.OpenRead("tg.jpg"))
//        //        {
//        //            InputOnlineFile? inputOnlineFile = new InputOnlineFile(imageFile, "Screenshot.jpg");
//        //            if (_UserId is not null)
//        //            {
//        //                SendPhoto("", inputOnlineFile);
//        //            }
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        if (ex.Message.Contains("being used by another process"))
//        //        {
//        //            SendMessage("Whoo, not so fast");
//        //        }
//        //        else
//        //        {
//        //            SendMessage("Something happend:" + ex.Message.ToString() + " cannot take screen.");
//        //        }
//        //        Logger.AddLogRecord(ex.Message.ToString(), Logger.Status.EXEPTION, ex);
//        //    }
//        //}

//        private void ProcessUpdate(global::Telegram.Bot.Types.Update update)
//        {
//            switch (update.Type)
//            {
//                case UpdateType.Message:
//                    {
//                        string? text = update?.Message?.Text;
//                        _LastSendedMessage = update.Message.MessageId;
//                        switch (text)
//                        {
//                            case GetScreenshotBtnText:
//                                {
//                                    _BotClient?.DeleteMessageAsync(_UserId, _LastSendedMessage);
//                                    TakeScreenshot("tg.jpg");
//                                    //GetScreenshot();
//                                }
//                                break;
//                            default:
//                                SendMessage("Choose another action");
//                                break;
//                        }
//                        break;
//                    }
//                default:
//                    SendMessage("This type of message is not supported");
//                    break;
//            }
//        }

//        private static IReplyMarkup Buttons()
//        {
//            var Keyboard = new List<List<KeyboardButton>>
//                {
//                    new List<KeyboardButton> {new KeyboardButton(GetScreenshotBtnText)}
//                };
//            return new ReplyKeyboardMarkup(Keyboard);
//        }
//    }
//}
