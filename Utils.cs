using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EldoradoBot
{
    public class Utils
    {
        public static readonly string _coreTokenString = "abcdefghijklmnopqrstuvwxyzAQWSZXDERFCVGTYHBNJUIKMLOP1234567890";

        public static readonly int _tokenLength = 20;

        private static object _locker = new();

        /// <summary>
        /// Returns a random string of given length and of given characters(string)
        /// </summary>
        /// <param name="length"></param>
        /// <param name="valid"></param>
        /// <returns></returns>
        public static string RandomString(int length, string valid)
        {
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }
            return res.ToString();
        }

        public static string GenerateToken()
        {
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];
                int length = _tokenLength;
                while (length -- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(_coreTokenString[(int)(num % (uint)_coreTokenString.Length)]);
                }
            }
            return res.ToString();
        }

        public static List<string>? ReadAllAccs(string fileWithAccsName)
        {
            if (fileWithAccsName is not null)
            {
                string[] accsLines = File.ReadAllLines($"{Environment.CurrentDirectory}\\Accounts\\{fileWithAccsName}.txt");
                return accsLines.ToList();
            }
            return null;
        }

        public static void ChangeLineInAFile(string text, string path, int lineToChangeIndex)
        {
            lock (_locker)
            {
                string[] arrLine = File.ReadAllLines(path);
                arrLine[lineToChangeIndex - 1] = text;
                File.WriteAllLines(path, arrLine);
            }
        }

        public static void ReWriteAFile(List<List<string>> text, string path)
        {
            lock (_locker)
            {
                File.WriteAllText(path, String.Empty);
                StreamWriter streamWriter = File.AppendText(path);
                foreach (List<string> line in text)
                {
                    string newline = "";
                    for (int i = 0; i <= line.Count-1; i++)
                    {
                        if (i == line.Count-1)
                        {
                            newline += $"{line[i]}";
                        }
                        else
                        {
                            newline += $"{line[i]}:";
                        }
                    }
                    streamWriter.WriteLine(newline);
                }
                streamWriter.Close();
            }
        }

        public class GameAccOffer
        {
            public string? _OfferName { get; set; }

            public OfferSignature? _OfferSignature { get; set; }   
            
            public string? _FileToGetAccFromName { get; set; }
            
            public string? _OfferSampleJsonFileName { get; set; }
           
            public List<List<string>>? _AccInfoPositions { get; set; }
            
            public string? _DelimiterForGetAccFile { get; set; }

            public string? _MaxAccsToListOnEldorado { get; set; }

            public GameAccOffer(string? offerName, OfferSignature? offerSignature, string? fileToGetAccFromName, string? offerSampleJsonFileName, List<List<string>>? accInfoPositions, string? delimiterForGetAccFile, string? maxAccsToListOnEldorado/*, string? number*/)
            {
                _OfferName = offerName;
                _OfferSignature = offerSignature;
                _FileToGetAccFromName = fileToGetAccFromName;
                _OfferSampleJsonFileName = offerSampleJsonFileName;
                _AccInfoPositions = accInfoPositions;
                _DelimiterForGetAccFile = delimiterForGetAccFile;
                _MaxAccsToListOnEldorado = maxAccsToListOnEldorado;
            }
        }

        public class OfferSignature
        {
            public string? _OfferItemId { get; set; }
            public List<string>? _OfferTradeEnviromentValues { get; set; }

            public OfferSignature(string? offerItemId, List<string>? offerTradeEnviromentValues)
            {
                _OfferItemId = offerItemId;
                _OfferTradeEnviromentValues = offerTradeEnviromentValues;
            }
        }
    }
}
