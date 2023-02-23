using EldoradoBot;
using System.Net;
using System.Text.RegularExpressions;

Console.Title = "Eldorado Shop Bot";

try
{
    bool refreshTokenIsGood = true;
    ConfigHandler? configInfo = new ConfigHandler()?.Read();

    if (configInfo?.OffersInfo?.OffersNames is not null && configInfo?.TelegramBotToken is not null && configInfo?.UsedId is not null)
    {
        ITelegramBot telegramBot = new JustNotifyTelegramBot();
        if (telegramBot.Init(configInfo.TelegramBotToken, configInfo.UsedId))
        {
            Logger.Init(telegramBot);
        }

        List<Utils.GameAccOffer> Offers = new List<Utils.GameAccOffer>() { };
        for (int i = 0; i < configInfo?.OffersInfo?.OffersNames.Count(); i++)
        {
            Offers.Add(new Utils.GameAccOffer(configInfo?.OffersInfo?.OffersNames[i], configInfo?.OffersInfo?.FileToGetAccsFromNames[i], configInfo?.OffersInfo?.OffersSamplesJsonFileNames[i], configInfo?.OffersInfo?.AccInfoPositions[i], configInfo?.OffersInfo?.DelimitersForGetAccsFiles[i], configInfo?.OffersInfo?.MaxAccsToListOnEldorado[i]));
        }
        Eldorado eldorado = new Eldorado();

        if (configInfo?.ChatLink is not null && eldorado.Init(configInfo))
        {
            string link = configInfo.ChatLink;

            //Start checking for messages and disputes 
            Thread eldoradoDataRenewingthread = new Thread(() => { eldorado.MessageChecking(link, refreshTokenIsGood); });
            eldoradoDataRenewingthread.Start();



            List<Eldorado.AccOnEldorado> refreshedAccs = new List<Eldorado.AccOnEldorado>();
            while (refreshTokenIsGood)
            {
                // read all accs from files
                List<List<List<string>>> AllAccsBase = new List<List<List<string>>>() { };
                foreach (Utils.GameAccOffer offer in Offers)
                {
                    if (offer._FileToGetAccFromName is not null)
                    {
                        var accs = Utils.ReadAllAccs(offer._FileToGetAccFromName);
                        List<List<string>> acctmp = new List<List<string>>() { };
                        foreach (var acc in accs)
                        {
                            var tempRes = acc.Split($"{offer._DelimiterForGetAccFile}").ToList();
                            if (!tempRes[tempRes.Count - 1].Contains("#"))
                            {
                                tempRes.Add("--------");
                                tempRes.Add($"#{Utils.GenerateToken()}");
                            }
                            acctmp.Add(tempRes);
                        }
                        AllAccsBase.Add(acctmp);
                    }
                }
                Logger.AddLogRecord($"Read accs for {AllAccsBase.Count} offer types", Logger.Status.OK);

                ///////
                //var baseAddress = new Uri("https://www.eldorado.gg");
                //var cookieContainer = new CookieContainer();
                //using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                //using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                //{
                //    var content = new FormUrlEncodedContent(new[]
                //    {
                //        new KeyValuePair<string, string>("foo", "bar"),
                //        new KeyValuePair<string, string>("baz", "bazinga"),
                //    });
                //    cookieContainer.Add(baseAddress, new Cookie("CookieName", "cookie_value"));
                //    var result = await client.PostAsync("/test", content);
                //    result.EnsureSuccessStatusCode();
                //}
                //var baseAddress = new Uri("https://www.eldorado.gg");
                //using (var handler = new HttpClientHandler { UseCookies = false })
                //using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                //{
                //    var message = new HttpRequestMessage(HttpMethod.Get, "/test");
                //    message.Headers.Add("Cookie", "cookie1=value1; cookie2=value2");
                //    var result = await client.SendAsync(message);
                //    result.EnsureSuccessStatusCode();
                //}
                //string URL = "https://www.eldorado.gg"; 
                //using (HttpClientHandler handler = new HttpClientHandler { AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.All })
                //{
                //    using (var clnt = new HttpClient(handler))
                //    {
                //        clnt.
                //        using (HttpResponseMessage resp = clnt.GetAsync(URL).Result)
                //        {
                //            if (resp.IsSuccessStatusCode)
                //            {
                //                var html = resp.Content.ReadAsStringAsync().Result;
                //                if (!string.IsNullOrEmpty(html))
                //                {
                //                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                //                    doc.LoadHtml(html);
                //                }
                //            }
                //        }
                //    }
                //}
                ////////


                

                var accsInfo = eldorado.GetAllOffersInfo();
                if (accsInfo is not null)
                {
                    //getting all accs from eldorado
                    List<List<string>> allAccsFromEldorado = new List<List<string>>() { };
                    List<List<string>> closedAccs = new List<List<string>>() { };
                    List<List<string>> activeAccs = new List<List<string>>() { };
                    List<List<string>> pausedAccs = new List<List<string>>() { };
                    foreach (var acc in accsInfo)
                    {
                        Regex regex = new Regex(@"[#][a-zA-Z\d]{20}");
                        if (acc.description is not null)
                        {
                            allAccsFromEldorado.Add(new List<string>() { acc.offerState, regex.Match(acc.description.ToString()).ToString() });
                            if (acc.offerState == "Closed")
                            {
                                closedAccs.Add(new List<string>() { acc.offerState, regex.Match(acc.description).ToString(), acc.id, acc.pricePerUnit.amount.ToString(), acc.pricePerUnit.currency.ToString(), acc.offerTitle.ToString() });
                            }
                            if (acc.offerState == "Active")
                            {
                                activeAccs.Add(new List<string>() { acc.offerState, regex.Match(acc.description).ToString(), acc.id, acc.pricePerUnit.amount.ToString(), acc.pricePerUnit.currency.ToString() });
                            }
                            if (acc.offerState == "Paused")
                            {
                                pausedAccs.Add(new List<string>() { acc.offerState, regex.Match(acc.description).ToString(), acc.id, acc.pricePerUnit.amount.ToString(), acc.pricePerUnit.currency.ToString() });
                            }
                        }
                    }
                    Logger.AddLogRecord($"Eldorado => Total: {allAccsFromEldorado.Count} Active: {activeAccs.Count} Paused: {pausedAccs.Count} Closed: {closedAccs.Count} ", Logger.Status.OK);


                    //Changing the data in txt file
                    foreach (var accGroup in AllAccsBase)
                    {
                        foreach (var acc in accGroup)
                        {
                            if (allAccsFromEldorado.FirstOrDefault(a => a[1] == acc[acc.Count - 1]) == null)
                            {
                                if (acc[acc.Count - 2] != "Closed")
                                {
                                    acc[acc.Count - 2] = "--------";
                                }
                            }
                        }
                    }

                    //deleting all closed acc from eldorado and adding this info to acc variable
                    int deletedCount = 0;
                    foreach (var closedAcc in closedAccs)
                    {
                        if (closedAcc[1] == "")
                        {
                            if (eldorado.DeleteAccOffer(closedAcc[2]))
                            {
                                deletedCount++;
                            }
                            Logger.AddLogRecord($" ✅Sold {closedAcc[5]} for {closedAcc[3]}{closedAcc[4]}", Logger.Status.OK, true);
                        }
                        else
                        {
                            int gameAccGroupNumber = 0;
                            foreach (var gameAccsGroup in AllAccsBase)
                            {

                                var soldAcc = gameAccsGroup.FirstOrDefault(acc => acc[acc.Count - 1] == closedAcc[1]);// if tokens are the same
                                if (soldAcc != null)
                                {
                                    int accForChange = gameAccsGroup.FindIndex(acc => acc[acc.Count - 1] == soldAcc[soldAcc.Count - 1]);
                                    gameAccsGroup[accForChange][gameAccsGroup[accForChange].Count() - 2] = "Closed";
                                    if (eldorado.DeleteAccOffer(closedAcc[2]))
                                    {
                                        deletedCount++;
                                    }
                                    Logger.AddLogRecord($" ✅Sold {Offers[gameAccGroupNumber]._OfferName} ({closedAcc[1]}) for {closedAcc[3]}{closedAcc[4]}", Logger.Status.OK, true);
                                }
                                Thread.Sleep(1000);
                                gameAccGroupNumber++;
                            } 
                        }
                    }
                    Logger.AddLogRecord($"{deletedCount} closed accs was deleted", Logger.Status.OK);


                    // list new offers til reach max num of accs if there is not enought
                    int gameAccGroupNumber1 = 0;
                    foreach (var gameAccsGroup in AllAccsBase)
                    {
                        int accsOnEldoradoInThisGroup = 0;
                        foreach (var acc in gameAccsGroup)
                        {
                            if (acc[acc.Count - 2] != "--------" && acc[acc.Count - 2] != "Closed")
                            {
                                accsOnEldoradoInThisGroup++;
                            }
                        }
                        Logger.AddLogRecord($"Need to add {Convert.ToInt32(Offers[gameAccGroupNumber1]._MaxAccsToListOnEldorado) - accsOnEldoradoInThisGroup} accs", Logger.Status.OK);
                        int count = 0;
                        while (accsOnEldoradoInThisGroup < Convert.ToInt32(Offers[gameAccGroupNumber1]._MaxAccsToListOnEldorado) && count <= 5)
                        {
                            var newAcc = gameAccsGroup.FirstOrDefault(acc => acc[acc.Count - 2] == "--------");
                            if (newAcc != null)
                            {
                                if (eldorado.CreateNewOfferFromFile(Offers[gameAccGroupNumber1], newAcc))
                                {
                                    accsOnEldoradoInThisGroup++;
                                    int accForChange = gameAccsGroup.FindIndex(acc => acc[acc.Count - 1] == newAcc[newAcc.Count - 1]);
                                    gameAccsGroup[accForChange][gameAccsGroup[accForChange].Count() - 2] = "OnEldorado";
                                }
                                else
                                {
                                    count++;
                                }
                                Thread.Sleep(3000);
                            }
                            else
                            {
                                count++;
                            }
                        }
                        if (count >= 5)
                        {
                            Offers[gameAccGroupNumber1]._MaxAccsToListOnEldorado = accsOnEldoradoInThisGroup.ToString();
                            Logger.AddLogRecord($"No accs available, now max for this offer type is {Offers[gameAccGroupNumber1]._MaxAccsToListOnEldorado}", Logger.Status.WARN);
                        }
                        Logger.AddLogRecord($"{accsOnEldoradoInThisGroup} acc now on {Offers[gameAccGroupNumber1]._OfferName} listing", Logger.Status.OK);
                        gameAccGroupNumber1++;
                    }


                    //write to a file
                    int gameAccGroupNumber2 = 0;
                    foreach (var gameAccsGroup in AllAccsBase)
                    {
                        Utils.ReWriteAFile(gameAccsGroup, $"{Environment.CurrentDirectory}\\Accounts\\{Offers[gameAccGroupNumber2]._FileToGetAccFromName}.txt");
                        gameAccGroupNumber2++;
                    }
                    Logger.AddLogRecord($"Data was saved to the file", Logger.Status.OK);


                    
                    accsInfo = eldorado.GetAllOffersInfo();
                    if (accsInfo != null)
                    {
                        List<Eldorado.AccOnEldorado> AccsitemIds = new List<Eldorado.AccOnEldorado>();
                        List<Eldorado.AccOnEldorado> itemIds = new List<Eldorado.AccOnEldorado>();

                        foreach (var accs in accsInfo)
                        {
                            string tmp1 = "";
                            string tmp2 = "";
                            if (accs.tradeEnvironmentValues.Count != 0)
                            {
                                tmp1 = accs.tradeEnvironmentValues[0].id;
                            }
                            if (accs.offerAttributeIdValues.Count != 0)
                            {
                                tmp2 = accs.offerAttributeIdValues[0].ToString();
                            }
                            AccsitemIds.Add(new Eldorado.AccOnEldorado(accs.itemId, tmp1, tmp2));
                        }

                        itemIds = AccsitemIds.GroupBy(x => x).Select(y => y.First()).DistinctBy(c => new { c._itemId, c._offerAttributeIdValues, c._tradeEnvironmentValues }).ToList();
                        bool isAllAccsPaused = false;

                        if (refreshedAccs.Count == 0)
                        {
                            foreach (var itemId in itemIds)
                            {
                                refreshedAccs.Add(itemId);
                            }
                        }

                        var accToRefresh = refreshedAccs[0];
                        isAllAccsPaused = eldorado.PauseGameOffers(accToRefresh);
                        if (isAllAccsPaused)
                        {
                            Logger.AddLogRecord($"Waiting 1 minute...", Logger.Status.OK);
                            Thread.Sleep(60000); // 1 min
                        }
                        else
                        {
                            Logger.AddLogRecord($"Too many requests...", Logger.Status.WARN);
                        }
                        int? totalAccsResumed = 0;
                        totalAccsResumed += eldorado.ResumeGameOffers(accToRefresh);
                        refreshedAccs.RemoveAt(0);
                        Logger.AddLogRecord($"{totalAccsResumed} accs was resumed", Logger.Status.OK);
                        Logger.AddLogRecord($"Waiting {configInfo.OfferTime} minute...", Logger.Status.OK);
                        Thread.Sleep(Convert.ToInt32(configInfo.OfferTime) * 60 * 1000);
                        Logger.AddLogRecord("-------------------", Logger.Status.OK);

                    }
                    //////////

                    //Logger.AddLogRecord($"Waiting {configInfo.OfferTime} minute...", Logger.Status.OK);
                    //Thread.Sleep(Convert.ToInt32(configInfo.OfferTime) * 60 * 1000);
                    //Logger.AddLogRecord("-------------------", Logger.Status.OK);
                }
                else
                {
                    eldorado.RefreshSession();
                }
            }
        }
        else
        {
            Logger.AddLogRecord("Failed to init, wrong refresh token, please get new refreshToken", Logger.Status.BAD, true);
        }
    }
    else
    {
        Logger.AddLogRecord("Bad config file", Logger.Status.BAD);
    }
}
catch (Exception ex)
{
    Logger.AddLogRecord($"Exeption in main service {ex}", Logger.Status.EXEPTION, true);
}