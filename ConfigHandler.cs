using System.Text.Json;
using System.Text.Json.Serialization;

namespace EldoradoBot
{
    public class OffersInfo
    {
        [JsonInclude]
        public List<string>? OffersNames { get; set; }
        [JsonInclude]
        public Signature? Signature { get; set; }
        [JsonInclude]
        public List<string>? FileToGetAccsFromNames { get; set; }
        [JsonInclude]
        public List<string>? OffersSamplesJsonFileNames { get; set; }
        [JsonInclude]
        public List<List<List<string>>>? AccInfoPositions { get; set; }
        [JsonInclude]
        public List<string>? DelimitersForGetAccsFiles { get; set; }
        [JsonInclude]
        public List<string>?  MaxAccsToListOnEldorado { get; set; }
        //[JsonInclude]
        //public List<string>? Number { get; set; }
    }

    public class Signature
    {
        [JsonPropertyName("ItemIds")]
        public List<string>? ItemIds { get; set; }

        [JsonPropertyName("TradeEnviromentValues")]
        public List<List<string>>? TradeEnviromentValues { get; set; }
    }

    public class ConfigHandler 
    {
        [JsonInclude]
        public string? EldoradoRefreshToken { get; set; }
        [JsonInclude]
        public string? ChatLink { get; set; }
        [JsonInclude]
        public string? TelegramBotToken { get; set; }
        [JsonInclude]
        public string? UsedId { get; set; }
        [JsonInclude]
        public string? OfferTime { get; set; }
        [JsonInclude]
        public OffersInfo? OffersInfo { get; set; }


        private static readonly string ConfigPath = $"{Environment.CurrentDirectory}\\Config.json";

        public ConfigHandler? Read()
        {
            if (File.Exists(ConfigPath))
            {
                ConfigHandler? configFile;
                using (FileStream fileStream = new FileStream(ConfigPath, FileMode.Open))
                {
                    try
                    {
                        configFile = JsonSerializer.Deserialize<ConfigHandler>(fileStream);
                        if (configFile is not null)
                        {
                            if (configFile.OffersInfo?.OffersNames?.Count == configFile?.OffersInfo?.FileToGetAccsFromNames?.Count &&
                                configFile?.OffersInfo?.OffersNames?.Count == configFile?.OffersInfo?.OffersSamplesJsonFileNames?.Count &&
                                configFile?.OffersInfo?.OffersNames?.Count == configFile?.OffersInfo?.AccInfoPositions?.Count &&
                                configFile?.OffersInfo?.OffersNames?.Count == configFile?.OffersInfo?.DelimitersForGetAccsFiles?.Count &&
                                configFile?.OffersInfo?.OffersNames?.Count == configFile?.OffersInfo?.MaxAccsToListOnEldorado?.Count /*&&*/
                                /*configFile?.OffersInfo?.OffersNames?.Count == configFile?.OffersInfo?.Number?.Count*/)
                            {
                                return configFile;
                            }
                            else
                            {
                                Logger.AddLogRecord("Bad config file", Logger.Status.BAD);
                                return null;
                            }
                        }
                        else
                        {
                            Logger.AddLogRecord("Failed to read config file", Logger.Status.BAD);
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLogRecord("Failed to read config file with exception", Logger.Status.EXEPTION,ex);
                        return null;
                    }
                }
            }
            Logger.AddLogRecord("Config readed", Logger.Status.OK);
            return null;
        }
    }
}

//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace EldoradoBot
//{

//    public class OfferBluprint
//    {
//        public string? Name { get; set; }

//        public string? FileToGetAccsFrom { get; set; }

//        public string? OfferSampleJsonFile { get; set; }

//        public List<List<string>>? AccInfoPositions { get; set; }

//        public string DelimitersForGetAccsFiles { get; set; }

//        public OfferBluprint(string name, string fileToGetAccsFrom, string offerSampleJsonFile, List<List<string>>? accInfoPositions, string delimitersForGetAccsFiles)
//        {
//            Name = name;
//            FileToGetAccsFrom = fileToGetAccsFrom;
//            OfferSampleJsonFile = offerSampleJsonFile;
//            AccInfoPositions = accInfoPositions;
//            DelimitersForGetAccsFiles = delimitersForGetAccsFiles;
//        }
//    }
//    public class OffersInfo
//    {
//        public List<string>? OffersNames { get; set; }

//        public List<string>? FileToGetAccsFromNames { get; set; }

//        public List<string>? OffersSamplesJsonFileNames { get; set; }

//        public List<List<List<string>>>? AccInfoPositions { get; set; }

//        public List<string>? DelimitersForGetAccsFiles { get; set; }
//    }

//    public class Config
//    {
//        [JsonInclude]
//        public string? EldoradoRefreshToken { get; set; }
//        [JsonInclude]
//        public string? TelegramBotToken { get; set; }
//        [JsonInclude]
//        public string? UsedId { get; set; }
//        [JsonInclude]
//        public OffersInfo? Offers { get; set; }
//    }

//    public static class ConfigHandler
//    {
//        public static string? EldoradoRefreshToken { get; set; }

//        public static List<OfferBluprint>? Offers { get; set; }

//        public static string? TelegramBotToken { get; set; }

//        public static string? UsedId { get; set; }


//        private static readonly string ConfigPath = $"{Environment.CurrentDirectory}\\Config.json";

//        public static bool Read()
//        {
//            if (File.Exists(ConfigPath))
//            {
//                Config? configFile;
//                using (FileStream fileStream = new FileStream(ConfigPath, FileMode.Open))
//                {
//                    try
//                    {
//                        configFile = JsonSerializer.Deserialize<Config>(fileStream);
//                        if (configFile is not null)
//                        {
//                            if (configFile?.Offers?.OffersNames?.Count == configFile?.Offers?.FileToGetAccsFromNames?.Count &&
//                                configFile?.Offers?.OffersNames?.Count == configFile?.Offers?.OffersSamplesJsonFileNames?.Count &&
//                                configFile?.Offers?.OffersNames?.Count == configFile?.Offers?.AccInfoPositions?.Count &&
//                                configFile?.Offers?.OffersNames?.Count == configFile?.Offers?.DelimitersForGetAccsFiles?.Count)
//                            {
//                                List<OfferBluprint> offers = new List<OfferBluprint>();
//                                for (int i = 0; i < configFile?.Offers?.OffersNames?.Count; i++)
//                                {
//                                    //var tempRes = new List<List<string>>() { };
//                                    //foreach (var o in configFile?.Offers?.AccInfoPositions[i])
//                                    //{
//                                    //    tempRes.Add(o[0],o[1]);
//                                    //    tempRes.Add(o[0], o[1]);
//                                    //}
//                                    offers.Add(new OfferBluprint(configFile.Offers.OffersNames[i], configFile.Offers.FileToGetAccsFromNames[i], configFile.Offers.OffersSamplesJsonFileNames[i], configFile.Offers.AccInfoPositions[i], configFile.Offers.DelimitersForGetAccsFiles[i]));
//                                }
//                                Offers = offers;
//                                EldoradoRefreshToken = configFile?.EldoradoRefreshToken;
//                                TelegramBotToken = configFile?.TelegramBotToken;
//                                UsedId = configFile?.UsedId;
//                            }
//                            else
//                            {
//                                Logger.AddLogRecord("Bad config file", Logger.Status.BAD);
//                                return false;
//                            }
//                        }
//                        else
//                        {
//                            Logger.AddLogRecord("Failed to read config file", Logger.Status.BAD);
//                            return false;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        Logger.AddLogRecord("Failed to read config file with exception", Logger.Status.EXEPTION, ex);
//                        return false;
//                    }
//                }
//            }
//            Logger.AddLogRecord("Config readed", Logger.Status.OK);
//            return true;
//        }
//    }
//}

