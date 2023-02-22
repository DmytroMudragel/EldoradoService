using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EldoradoBot.Requests
{
    public class PauseAllOffersRequest
    {
        public string _Url = "https://www.eldorado.gg/api/offers/me/pauseAllActive/?itemTreeId=32-1-0&offerAttributeIdsCsv=0-0&tradeEnvironmentId=1&offerType=Account";

        public string _itemId;

        public string _offerAttributeIds;

        public string _tradeEnvironmentId;

        public string _offerType = "Account";

        public PauseAllOffersRequest(string itemId, string offerType,string offerAttributeIds, string tradeEnvironmentId)
        {
            _itemId = itemId;
            _offerType = offerType;
            _offerAttributeIds = offerAttributeIds;
            _tradeEnvironmentId = tradeEnvironmentId;
            _Url = $"https://www.eldorado.gg/api/offers/me/pauseAllActive/?itemTreeId={_itemId}&offerAttributeIdsCsv={_offerAttributeIds}&tradeEnvironmentId={_tradeEnvironmentId}&offerType={_offerType}";
        }
    } 
}
