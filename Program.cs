using EldoradoBot;
using System.Net;
using System.Text.RegularExpressions;

Console.Title = "Eldorado Shop Bot";
//try
//{
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
            Offers.Add(new Utils.GameAccOffer(configInfo?.OffersInfo?.OffersNames[i], new Utils.OfferSignature(configInfo?.OffersInfo?.Signature.ItemIds[i], configInfo?.OffersInfo?.Signature.TradeEnviromentValues[i]),
                configInfo?.OffersInfo?.FileToGetAccsFromNames[i], configInfo?.OffersInfo?.OffersSamplesJsonFileNames[i],
                configInfo?.OffersInfo?.AccInfoPositions[i], configInfo?.OffersInfo?.DelimitersForGetAccsFiles[i], configInfo?.OffersInfo?.MaxAccsToListOnEldorado[i]));
        }
        Eldorado eldorado = new Eldorado();

        if (configInfo?.ChatLink is not null && eldorado.Init(configInfo))
        {
            //Start checking for messages and disputes 
            eldorado.StartMessageAndDisputsChecking(configInfo.ChatLink, refreshTokenIsGood);







            int lastDisputsCount = 0;
            List<Eldorado.AccOnEldorado> refreshedAccs = new List<Eldorado.AccOnEldorado>();
            while (refreshTokenIsGood)
            {
                //try
                //{
                    // read all accs from files
                    List<List<List<string>>> AllAccsBase = eldorado.ReadAllAccsFromLocalFile(Offers);

                    var accsInfo = eldorado.GetAllOffersInfoFromEldorado();
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
                                List<string> tmp = new List<string> { acc?.offerState, regex.Match(acc.description.ToString()).ToString(), acc?.id, acc?.pricePerUnit?.amount?.ToString(), acc?.pricePerUnit?.currency?.ToString(), acc?.offerTitle?.ToString(), acc.itemId };
                                if (acc?.tradeEnvironmentValues.Count == 0)
                                {
                                    tmp.Add(null);
                                }
                                else
                                {
                                    foreach (var value in acc?.tradeEnvironmentValues)
                                    {
                                        tmp.Add(value.id?.ToString());
                                    }
                                }
                                allAccsFromEldorado.Add(tmp);
                                if (acc.offerState == "Closed")
                                {
                                    closedAccs.Add(tmp);                                   
                                }
                                if (acc.offerState == "Active")
                                {
                                    activeAccs.Add(tmp);
                                }
                                if (acc.offerState == "Paused")
                                {
                                    pausedAccs.Add(tmp);
                                }
                            }
                        }
                        Logger.AddLogRecord($"Eldorado => Total: {allAccsFromEldorado.Count} Active: {activeAccs.Count} Paused: {pausedAccs.Count} Closed: {closedAccs.Count} ", Logger.Status.OK);

                        //Check for disputes
                        int res = eldorado.GetActivities();
                        if (res == 0)
                        {
                            lastDisputsCount = 0;
                        }
                        if (res > 0 && res > lastDisputsCount)
                        {
                            Logger.AddLogRecord($"{res} new Disputed order", Logger.Status.OK, true);
                            lastDisputsCount = lastDisputsCount + res;
                        }

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
                                    List<List<string>> currentGameAccsGroupForSold = eldorado?.ReadSpecificAccsFromLocalFile(Offers[gameAccGroupNumber]);
                                    var soldAcc = currentGameAccsGroupForSold?.FirstOrDefault(acc => acc[acc.Count - 1] == closedAcc[1]);// if tokens are the same
                                    if (soldAcc != null && !soldAcc.Contains("Closed") && closedAcc[6] == Offers[gameAccGroupNumber]._OfferSignature._OfferItemId && closedAcc[7] == Offers[gameAccGroupNumber]._OfferSignature._OfferTradeEnviromentValues[0])
                                    {
                                        int accForChange = currentGameAccsGroupForSold.FindIndex(acc => acc[acc.Count - 1] == soldAcc[soldAcc.Count - 1]);
                                        currentGameAccsGroupForSold[accForChange][currentGameAccsGroupForSold[accForChange].Count() - 2] = "Closed";
                                        if (eldorado.DeleteAccOffer(closedAcc[2]))
                                        {
                                            deletedCount++;
                                        }
                                        Logger.AddLogRecord($" ✅Sold {Offers[gameAccGroupNumber]._OfferName} ({closedAcc[1]}) for {closedAcc[3]}{closedAcc[4]}", Logger.Status.OK, true);
                                        Utils.ReWriteAFile(currentGameAccsGroupForSold, $"{Environment.CurrentDirectory}\\Accounts\\{Offers[gameAccGroupNumber]._FileToGetAccFromName}.txt");
                                        Logger.AddLogRecord($"Data was saved to the file", Logger.Status.OK);
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
                            List<List<string>> currentGameAccsGroup = eldorado.ReadSpecificAccsFromLocalFile(Offers[gameAccGroupNumber1]);
                            int accsOnEldoradoInThisGroup = 0;
                            foreach (var acc in allAccsFromEldorado)
                            {
                                if (acc[6] != null && acc[6] == Offers[gameAccGroupNumber1]._OfferSignature._OfferItemId && acc[7] == Offers[gameAccGroupNumber1]._OfferSignature._OfferTradeEnviromentValues[0])
                                {
                                    accsOnEldoradoInThisGroup++;
                                }
                            }
                            Logger.AddLogRecord($"Need to add {Convert.ToInt32(Offers[gameAccGroupNumber1]._MaxAccsToListOnEldorado) - accsOnEldoradoInThisGroup} accs to {Offers[gameAccGroupNumber1]._OfferName}", Logger.Status.OK);
                            int count = 0;
                            while (accsOnEldoradoInThisGroup < Convert.ToInt32(Offers[gameAccGroupNumber1]._MaxAccsToListOnEldorado) && count <= 5)
                            {
                                var newAcc = currentGameAccsGroup.FirstOrDefault(acc => acc[acc.Count - 2] == "--------");
                                if (newAcc != null)
                                {
                                    if (eldorado.CreateNewOfferFromFile(Offers[gameAccGroupNumber1], newAcc))
                                    {
                                        accsOnEldoradoInThisGroup++;
                                        int accForChange = currentGameAccsGroup.FindIndex(acc => acc[acc.Count - 1] == newAcc[newAcc.Count - 1]);
                                        currentGameAccsGroup[accForChange][currentGameAccsGroup[accForChange].Count() - 2] = "OnEldorado";
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
                            Utils.ReWriteAFile(currentGameAccsGroup, $"{Environment.CurrentDirectory}\\Accounts\\{Offers[gameAccGroupNumber1]._FileToGetAccFromName}.txt");
                            Logger.AddLogRecord($"Data was saved to the file", Logger.Status.OK);
                            Thread.Sleep(10000);
                            gameAccGroupNumber1++;
                        }

                        //refreshing 1 of the acc type
                        accsInfo = eldorado.GetAllOffersInfoFromEldorado();
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
                    }
                    else
                    {
                        eldorado.RefreshSession();
                    }
                //}
                //catch (Exception ex)
                //{
                //    Logger.AddLogRecord($"Exeption in main loop {ex}", Logger.Status.EXEPTION, true);
                //}
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
//}
//catch (Exception ex)
//{
//    Logger.AddLogRecord($"Exeption in main service {ex}", Logger.Status.EXEPTION, true);
//}