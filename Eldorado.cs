using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using EldoradoBot.Responses;
using EldoradoBot.Requests;
using HtmlAgilityPack;
using System.Drawing;
using System.Windows;

namespace EldoradoBot
{
    public class Eldorado
    {
        public HttpClient? _client;

        private CookieContainer? _currentCookieContainer;

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:102.0) Gecko/20100101 Firefox/102.0";


        public class AccOnEldorado
        {
            public string _itemId = "";
            public string _tradeEnvironmentValues = "";
            public string _offerAttributeIdValues = "";

            public AccOnEldorado(string itemId, string tradeEnvironmentValues, string offerAttributeIdValues)
            {
                _itemId = itemId;
                _tradeEnvironmentValues = tradeEnvironmentValues;
                _offerAttributeIdValues = offerAttributeIdValues;
            }
        }

        public class EldoradoMessage
        {
            public string? _messageText { get; set; }
            public string? _buyerName { get; set; }
            public EldoradoMessage(string messageText, string buyerName)
            {
                _messageText = messageText;
                _buyerName = buyerName;
            }
        }

        public bool Init(ConfigHandler configHandler)
        {
            try
            {
                CookieContainer tmpContainer = new CookieContainer();
                tmpContainer.Add(new Cookie("__Host-EldoradoRefreshToken", configHandler.EldoradoRefreshToken, "/", "www.eldorado.gg"));
                _currentCookieContainer = tmpContainer;
                _client = new HttpClient(new HttpClientHandler
                {
                    CookieContainer = _currentCookieContainer,
                    AllowAutoRedirect = true,
                    UseCookies = true,
                    AutomaticDecompression = DecompressionMethods.All
                }, false);
                if (_client is not null)
                {
                    AllOffersInfoRequest allOffersInfoRequest = new AllOffersInfoRequest(1, 40, "Account");
                    HttpResponseMessage? httpResponseMessage = _client.GetAsync(allOffersInfoRequest._Url).Result;
                    if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        if (!RefreshSession())
                        {
                            return false;
                        }
                    }
                }
                //RefreshSession();
                Logger.AddLogRecord("Init ready", Logger.Status.OK);
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Init exeption: {ex}", Logger.Status.EXEPTION);
                return false;
            }
        }

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

        private HtmlAgilityPack.HtmlDocument? GetHtml(string url)
        {
            try
            {
                if (_client is not null)
                {
                    var response = _client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var html = response.Content.ReadAsStringAsync().Result;
                        if (!string.IsNullOrEmpty(html))
                        {
                            HtmlAgilityPack.HtmlDocument htmldoc = new HtmlAgilityPack.HtmlDocument();
                            htmldoc.LoadHtml(html);
                            return htmldoc;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Failed to get html: {ex}", Logger.Status.EXEPTION);
                return null;
            }
        }

        private List<EldoradoMessage>? GetUnreadMessages(string link)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument? htmlDoc = GetHtml(link);
            if (htmlDoc is not null)
            {
                List<EldoradoMessage>? messageList = new List<EldoradoMessage>();
                HtmlNodeCollection chats = htmlDoc.DocumentNode.SelectNodes(".//a[@class='ConversationListItem__conversation-link ConversationListItem__unread']");
                foreach (var chat in chats)
                {
                    string messageText = chat.SelectSingleNode(".//div[contains(@class,'ConversationListItem__message')]").SelectSingleNode(".//span[contains(@class,'Emojilinkistrippify')]").InnerText;
                    string buyerName = chat.GetAttributeValue("title", "");
                    if (!messageText.Contains("left the chat.") && !messageText.Contains("If you received goods or services"))
                    {
                        messageList.Add(new EldoradoMessage(messageText, buyerName));
                    }
                }
                return messageList;
            }
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Failed to get unread messages: {ex}", Logger.Status.EXEPTION);
                return null;
            }
            return null;
        }

        public void MessageChecking(string link, bool refreshTokenIsValid)
        {
            try
            {
                if ((link is not null) && (refreshTokenIsValid is true))
                {
                    int lastDisputsCount = 0;
                    List<EldoradoMessage>? messageHistory = new List<EldoradoMessage>();
                    while (refreshTokenIsValid)
                    {
                        //Check for disputes
                        int res = GetActivities();
                        if (res == 0)
                        {
                            lastDisputsCount = 0;
                        }
                        if (res > 0 && res > lastDisputsCount)
                        {
                            Logger.AddLogRecord($"{res} new Disputed order", Logger.Status.OK, true);
                            lastDisputsCount = lastDisputsCount + res;
                        }

                        //Check for messages
                        List<EldoradoMessage>? freshMessages = GetUnreadMessages(link);
                        if (freshMessages is not null)
                        {
                            foreach (EldoradoMessage message in freshMessages)
                            {
                                if (!messageHistory.Contains(message))
                                {
                                    Logger.AddLogRecord($"⚠️ New message from [{message._buyerName}]\n{message._messageText}", Logger.Status.OK, true);
                                    messageHistory.Add(message);
                                }
                            }                      
                        }

                        //Prevent message overflow
                        if (freshMessages is null && messageHistory.Count > 20)
                        {
                            messageHistory.Clear();
                        }

                        Thread.Sleep(5 * 60000);
                    }
                }
                else
                {
                    Logger.AddLogRecord($"Wrong token or link cannot check for unread messages", Logger.Status.BAD);
                }
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Error while checking unread messages: {ex}", Logger.Status.EXEPTION);
            }
        }



        public bool RefreshSession()
        {
            try
            {
                if (_client is not null)
                {
                    _client.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
                    RefreshSessionRequest refreshSessionRequest = new RefreshSessionRequest();
                    HttpResponseMessage? httpResponseMessage = _client.PostAsync(refreshSessionRequest._Url, null).Result;
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK && httpResponseMessage.Content != null)
                    {
                        HttpHeaders headers = httpResponseMessage.Headers;
                        string newCookie = "";
                        if (headers.TryGetValues("set-cookie", out IEnumerable<string>? values))
                        {
                            newCookie = values.First();
                        }
                        Regex regex = new Regex(@"\=(.*?)\;");
                        string res = regex.Match(newCookie).Groups[1].ToString();
                        if (_currentCookieContainer is not null)
                        {
                            _currentCookieContainer.Add(new Cookie("__Host-EldoradoIdToken", res, "/", "www.eldorado.gg"));
                            Logger.AddLogRecord("Session refreshed", Logger.Status.OK);
                            return true;
                        }
                    }
                }
                Logger.AddLogRecord("Failed refreshing", Logger.Status.BAD);
                return false;
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Refreshing exeption: {ex}", Logger.Status.EXEPTION);
                return false;
            }
        }

        public bool CreateNewOrder(string login, string password)
        {
            try
            {
                if (_client is not null)
                {
                    string json = File.ReadAllText($"{Environment.CurrentDirectory}\\Offer.json");
                    string res = json.Replace("Login : xxxxxxxxx", $"Login : {login}").Replace("Password : xxxxxxxxx", $"Password : {password}");
                    var httpContent = new StringContent(res, Encoding.UTF8, "application/json");
                    var httpResponse = _client.PostAsync(NewOffer._Url, httpContent).Result;
                    if (httpResponse.StatusCode == HttpStatusCode.Created && httpResponse.Content != null)
                    {
                        Logger.AddLogRecord("New offer created", Logger.Status.OK);
                        return true;
                    }
                }
                Logger.AddLogRecord("Failed creating new order", Logger.Status.BAD);
                return false;
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Creating new order exeption: {ex}", Logger.Status.EXEPTION);
                return false;
            }
        }


        public int GetActivities()
        {
            try
            {
                if (_client is not null)
                {
                    GetActivitiesRequest ActivitiesRequest = new GetActivitiesRequest();
                    HttpResponseMessage? httpResponseMessage = _client.GetAsync(ActivitiesRequest._Url).Result;
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK && httpResponseMessage.Content != null)
                    {
                        var jsonString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                        if (jsonString is not null)
                        {
                            GetActivitiesResponse.Root? activitiesInfo = System.Text.Json.JsonSerializer.Deserialize<GetActivitiesResponse.Root>(jsonString);
                            if (activitiesInfo is not null)
                            {
                                Logger.AddLogRecord("Got activities", Logger.Status.OK);
                                return activitiesInfo.disputedOrderCount;
                            }
                        }
                    }
                }
                Logger.AddLogRecord("Failed getting activities", Logger.Status.BAD);
                return -1;
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Get activities exeption: {ex}", Logger.Status.EXEPTION);
                return -1;
            }
        }

        public bool CreateNewOfferFromFile(Utils.GameAccOffer offerInfo, List<string> acc)
        {
            try
            {
                if (_client is not null)
                {
                    string json = File.ReadAllText($"{Environment.CurrentDirectory}\\JsonSamples\\{offerInfo._OfferSampleJsonFileName}.json");
                    json = json.Replace("(#)", acc[acc.Count - 1]);
                    if (offerInfo?._AccInfoPositions is not null)
                    {
                        foreach (var spot in offerInfo._AccInfoPositions)
                        {
                            json = json.Replace(spot[1], acc[Convert.ToInt32(spot[0])]);

                        }
                        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var httpResponse = _client.PostAsync(NewOffer._Url, httpContent).Result;
                        if (httpResponse.StatusCode == HttpStatusCode.Created && httpResponse.Content != null)
                        {
                            Logger.AddLogRecord($"New offer {offerInfo._OfferName} => {acc[acc.Count - 1]} created", Logger.Status.OK);
                            return true;
                        }
                    }
                }
                Logger.AddLogRecord("Failed creating new order", Logger.Status.BAD);
                return false;
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Creating new order exeption: {ex}", Logger.Status.EXEPTION);
                return false;
            }
        }

        public bool DeleteAccOffer(string itemId)
        {
            if (_client != null)
            {
                DeleteAccFromEldoradoRequest deleteAccFromEldoradoRequest = new DeleteAccFromEldoradoRequest(itemId);
                HttpResponseMessage? httpResponseMessage = _client.DeleteAsync(deleteAccFromEldoradoRequest._Url).Result;
                if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                {
                    Logger.AddLogRecord($"Acc {itemId} was deleted", Logger.Status.OK);
                    return true;
                }
            }
            return false;
        }

        public bool PauseGameOffers(AccOnEldorado itemId)
        {
            if (_client != null)
            {
                Regex regex = new Regex(@"\d-\d");
                var res = regex.Match(itemId._offerAttributeIdValues).ToString();
                PauseAllOffersRequest pauseOffersRequest = new PauseAllOffersRequest(itemId._itemId, "Account", res, itemId._tradeEnvironmentValues);
                HttpResponseMessage? httpResponseMessage = _client.PutAsync(pauseOffersRequest._Url, null).Result;
                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    Logger.AddLogRecord($"All {itemId._itemId}|{itemId._offerAttributeIdValues}|{itemId._tradeEnvironmentValues} was paused", Logger.Status.OK);
                    return true;
                }
            }
            return false;
        }

        public int? ResumeGameOffers(AccOnEldorado itemId)
        {
            if (_client != null)
            {
                Regex regex = new Regex(@"\d-\d");
                var res = regex.Match(itemId._offerAttributeIdValues).ToString();
                ResumeAllOffersRequest pauseOffersRequest = new ResumeAllOffersRequest(itemId._itemId, "Account", res, itemId._tradeEnvironmentValues);
                HttpResponseMessage? httpResponseMessage = _client.PutAsync(pauseOffersRequest._Url, null).Result;
                if (httpResponseMessage.StatusCode == HttpStatusCode.OK && httpResponseMessage.Content != null)
                {
                    var jsonString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    if (jsonString is not null)
                    {
                        ResumeAccsResponse.Root? info = System.Text.Json.JsonSerializer.Deserialize<ResumeAccsResponse.Root>(jsonString);
                        if (info is not null && info.totalOffersToResumeLimitReached == false)
                        {
                            return info.offersInGivenFilterCount;
                        }
                    }
                }
            }
            return null;
        }

        public List<AllAccsInfo.Result>? GetAllOffersInfo()
        {
            try
            {
                List<AllAccsInfo.Result> res = new List<AllAccsInfo.Result>();
                if (_client is not null)
                {
                    AllOffersInfoRequest allOffersInfoRequest = new AllOffersInfoRequest(1, 40, "Account");
                    HttpResponseMessage? httpResponseMessage = _client.GetAsync(allOffersInfoRequest._Url).Result;
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK && httpResponseMessage.Content != null)
                    {
                        var jsonString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                        if (jsonString is not null)
                        {
                            AllAccsInfo.Root? allAccsInfo = System.Text.Json.JsonSerializer.Deserialize<AllAccsInfo.Root>(jsonString);
                            if (allAccsInfo is not null && allAccsInfo.recordCount is not null)
                            {


                                for (int i = 1; i <= allAccsInfo.totalPages; i++)
                                {
                                    AllOffersInfoRequest allOffersInfoSecondRequest = new AllOffersInfoRequest(i, 40, "Account");
                                    HttpResponseMessage? httpResponseSecondMessage = _client.GetAsync(allOffersInfoSecondRequest._Url).Result;
                                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK && httpResponseSecondMessage.Content != null)
                                    {
                                        var secondJsonString = httpResponseSecondMessage.Content.ReadAsStringAsync().Result;
                                        if (secondJsonString is not null)
                                        {
                                            AllAccsInfo.Root? secondAllAccsInfo = System.Text.Json.JsonSerializer.Deserialize<AllAccsInfo.Root>(secondJsonString);
                                            if (secondAllAccsInfo is not null && secondAllAccsInfo.results is not null)
                                            {
                                                foreach (var result in secondAllAccsInfo.results)
                                                {
                                                    res.Add(result);
                                                }
                                            }

                                        }
                                    }
                                    Thread.Sleep(500);
                                }
                                if (res.Count == allAccsInfo.recordCount)
                                {
                                    Logger.AddLogRecord("Got all accs info", Logger.Status.OK);
                                    return res;
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.AddLogRecord("failed", Logger.Status.BAD);
                    }
                }
                Logger.AddLogRecord("Failed getting all accs info", Logger.Status.BAD);
                return null;
            }
            catch (Exception ex)
            {
                Logger.AddLogRecord($"Getting all accs info exeption: {ex}", Logger.Status.EXEPTION);
                return null;
            }
        }

        //#region Base functions for any window

        ///// <summary>
        ///// Parse text from image
        ///// </summary>
        ///// <param name="image"></param>
        ///// <returns></returns>
        //        public static string ParseBitmap(Bitmap image)
        //        {
        //            var Ocr = new IronTesseract();
        //            using (var Input = new OcrInput(image))
        //            {
        //                Input.Contrast();
        //                Input.Invert();
        //                var Result = Ocr.Read(Input);
        //                return Result.Text;
        //            }
        //        }

        //        /// <summary>
        //        /// Crop bitmap image to certain size 
        //        /// </summary>
        //        /// <param name="bitmap"></param>
        //        /// <param name="x"></param>
        //        /// <param name="y"></param>
        //        /// <param name="width"></param>
        //        /// <param name="height"></param>
        //        /// <returns></returns>
        //        public static Bitmap GetCroppedBitmap(Bitmap bitmap, int x, int y, int width, int height)
        //        {
        //            Bitmap retBitmap = null;
        //            using (var currentTile = new Bitmap(width, height))
        //            {
        //                using (var currentTileGraphics = Graphics.FromImage(currentTile))
        //                {
        //                    currentTileGraphics.Clear(System.Drawing.Color.Black);
        //                    var absentRectangleArea = new System.Drawing.Rectangle(x, y, width, height);
        //                    currentTileGraphics.DrawImage(bitmap, 0, 0, absentRectangleArea, GraphicsUnit.Pixel);
        //                }
        //                retBitmap = new Bitmap(currentTile);
        //            }
        //            return retBitmap;
        //        }

        //        /// <summary>
        //        /// Returns window rectangle or empty rectangle if there no window
        //        /// </summary>
        //        /// <param name="winToFindName"></param>
        //        /// <returns></returns>
        //        public static Rectangle GetWindowRect(string winToFindName, string windowToFindClass, bool needed)
        //        {
        //            lock (scrLock)
        //            {
        //                if (!IsTheWindowsExist(windowToFindClass))
        //                {
        //                    return Rectangle.Empty;
        //                }
        //                if (needed && (AutoItX.WinGetTitle("[ACTIVE]") != winToFindName))
        //                {
        //                    AutoItX.WinActivate(windowToFindClass);
        //                    AutoItX.WinGetTitle();
        //                }
        //                return AutoItX.WinGetPos(windowToFindClass);
        //            }
        //        }

        //        /// <summary>
        //        /// Check is there windows exists 
        //        /// </summary>
        //        /// <returns></returns>
        //        public static bool IsTheWindowsExist(string windowToFindClass)
        //        {
        //            lock (scrLock)
        //            {
        //                return AutoItX.WinExists(windowToFindClass) == 1;
        //            }
        //        }

        //        /// <summary>
        //        /// Return certain window screenshot as bitmap or null 
        //        /// </summary>
        //        /// <param name="windowToActivateName"></param>
        //        /// <param name="windowToFindClass"></param>
        //        /// <param name="screenshotLock"></param>
        //        /// <returns></returns>
        //        public static Bitmap GetWinScreenshot(string windowToActivateName, string windowToFindClass)
        //        {
        //            lock (scrLock)
        //            {
        //                Rectangle currWinRect = GetWindowRect(windowToActivateName, windowToFindClass, true);
        //                if (currWinRect == Rectangle.Empty)
        //                {
        //                    return null;
        //                }
        //                Bitmap bitmap = new Bitmap(currWinRect.Width, currWinRect.Height);
        //                using (Graphics graphics = Graphics.FromImage(bitmap))
        //                {
        //                    graphics.CopyFromScreen(new System.Drawing.Point(currWinRect.Left, currWinRect.Top), System.Drawing.Point.Empty, currWinRect.Size);
        //                }
        //                return bitmap;
        //            }
        //        }

        //        [DllImport("user32.dll", SetLastError = true)]
        //        [return: MarshalAs(UnmanagedType.Bool)]
        //        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        //        /// <summary>
        //        /// Return certain window screenshot even if it is not active as bitmap or null
        //        /// </summary>
        //        /// <param name="windowToActivateName"></param>
        //        /// <param name="windowToFindClass"></param>
        //        /// <param name="screenshotLock"></param>
        //        /// <returns></returns>
        //        /// 
        //        public static Bitmap GetWinScreenshotNotActive(string windowToActivateName, string windowToFindClass)
        //        {
        //            lock (scrLock)
        //            {
        //                Rectangle currWinRect = GetWindowRect(windowToActivateName, windowToFindClass, false);
        //                if (currWinRect == Rectangle.Empty)
        //                {
        //                    return null;
        //                }
        //                Bitmap B = new Bitmap(currWinRect.Width, currWinRect.Height);
        //                using (Graphics graphics = Graphics.FromImage(B))
        //                {
        //                    Bitmap bmp = new Bitmap(currWinRect.Size.Width, currWinRect.Size.Height, graphics);
        //                    Graphics memoryGraphics = Graphics.FromImage(bmp);
        //                    IntPtr dc = memoryGraphics.GetHdc();
        //                    var handle = AutoItX.WinGetHandle(windowToFindClass);
        //                    bool success = PrintWindow(handle, dc, 0);
        //                    memoryGraphics.ReleaseHdc(dc);
        //                    return bmp;
        //                }
        //            }
        //        }

        //        /// <summary>
        //        /// Gets whole screen screenshot
        //        /// </summary>
        //        /// <returns></returns>
        //        public static Bitmap GetScreenShot()
        //        {
        //            Rectangle bounds = new Rectangle(0, 0, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
        //            Bitmap Screenshot = new Bitmap(bounds.Width, bounds.Height);
        //            using (Graphics graphics = Graphics.FromImage(Screenshot))
        //            {
        //                graphics.CopyFromScreen(new System.Drawing.Point(bounds.Left, bounds.Top), System.Drawing.Point.Empty, bounds.Size);
        //            }
        //            return Screenshot;
        //        }

        //        /// <summary>Finds a matching image on the screen.</summary>
        //        ///     ''' <param name="bmpMatch">The image to find on the screen.</param>
        //        ///     ''' <param name="ExactMatch">True finds an exact match (slowerer on large images). False finds a close match (faster on large images).</param>
        //        ///     ''' <param name="bmpWhereFind">Picture where to find subimage.</param>
        //        ///     ''' <returns>Returns a Rectangle of the found image in sceen coordinates.</returns>
        //        public static Rectangle FindImageOnScreen(Bitmap bmpMatch, Bitmap bmpWhereFind, bool ExactMatch)
        //        {
        //            BitmapData ImgBmd = bmpMatch.LockBits(new Rectangle(0, 0, bmpMatch.Width, bmpMatch.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        //            BitmapData ScreenBmd = bmpWhereFind.LockBits(new Rectangle(0, 0, bmpWhereFind.Width, bmpWhereFind.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        //            byte[] ImgByts = new byte[(Math.Abs(ImgBmd.Stride) * bmpMatch.Height) - 1 + 1];
        //            byte[] ScreenByts = new byte[(Math.Abs(ScreenBmd.Stride) * bmpWhereFind.Height) - 1 + 1];

        //            Marshal.Copy(ImgBmd.Scan0, ImgByts, 0, ImgByts.Length);
        //            Marshal.Copy(ScreenBmd.Scan0, ScreenByts, 0, ScreenByts.Length);

        //            bool FoundMatch = false;
        //            Rectangle rct = Rectangle.Empty;
        //            int sindx, iindx;
        //            int spc, ipc;
        //            int skpx = Convert.ToInt32((bmpMatch.Width - 1) / (double)10);
        //            if (skpx < 1 | ExactMatch)
        //                skpx = 1;
        //            int skpy = Convert.ToInt32((bmpMatch.Height - 1) / (double)10);
        //            if (skpy < 1 | ExactMatch)
        //                skpy = 1;
        //            for (int si = 0; si <= ScreenByts.Length - 1; si += 3)
        //            {
        //                FoundMatch = true;
        //                for (int iy = 0; iy <= ImgBmd.Height - 1; iy += skpy)
        //                {
        //                    for (int ix = 0; ix <= ImgBmd.Width - 1; ix += skpx)
        //                    {
        //                        sindx = (iy * ScreenBmd.Stride) + (ix * 3) + si;
        //                        iindx = (iy * ImgBmd.Stride) + (ix * 3);
        //                        spc = Color.FromArgb(ScreenByts[sindx + 2], ScreenByts[sindx + 1], ScreenByts[sindx]).ToArgb();
        //                        ipc = Color.FromArgb(ImgByts[iindx + 2], ImgByts[iindx + 1], ImgByts[iindx]).ToArgb();
        //                        if (spc != ipc)
        //                        {
        //                            FoundMatch = false;
        //                            iy = ImgBmd.Height - 1;
        //                            ix = ImgBmd.Width - 1;
        //                        }
        //                    }
        //                }
        //                if (FoundMatch)
        //                {
        //                    double r = si / (double)(bmpWhereFind.Width * 3);
        //                    double c = bmpWhereFind.Width * (r % 1);
        //                    if (r % 1 >= 0.5)
        //                        r -= 1;
        //                    rct.X = Convert.ToInt32(c);
        //                    rct.Y = Convert.ToInt32(r);
        //                    rct.Width = bmpMatch.Width;
        //                    rct.Height = bmpMatch.Height;
        //                    break;
        //                }
        //            }
        //            bmpMatch.UnlockBits(ImgBmd);
        //            bmpWhereFind.UnlockBits(ScreenBmd);
        //            return rct;
        //        }

        //        #endregion
    }
}
