using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EldoradoBot.Requests
{
    public class ResumeAllOffersRequest
    {
        public string _Url = "https://www.eldorado.gg/api/offers/me/resumeLatestPaused/32-1-0/?offerAttributeIdsCsv=0-0&tradeEnvironmentId=0&offerType=Account&itemTreeId=32-1-0";

        public string _itemId;

        public string _offerType = "Account";

        public string _offerAttributeIds;

        public string _tradeEnvironmentId;

        public ResumeAllOffersRequest(string itemId, string offerType, string offerAttributeIds, string tradeEnvironmentId)
        {
            _itemId = itemId;
            _offerType = offerType;
            _offerAttributeIds = offerAttributeIds;
            if (_tradeEnvironmentId == "")
            {
                _tradeEnvironmentId = "0";
            }
            else
            {
                _tradeEnvironmentId = tradeEnvironmentId;
            }
            _Url = $"https://www.eldorado.gg/api/offers/me/resumeLatestPaused/{_itemId}/?offerAttributeIdsCsv={_offerAttributeIds}&tradeEnvironmentId={_tradeEnvironmentId}&offerType={_offerType}&itemTreeId={_itemId}";
            //_Url = $"https://www.eldorado.gg/api/offers/me/resumeLatestPaused/{_itemId}/?tradeEnvironmentId=0&offerType={_offerType}&itemTreeId={_itemId}";
            //_Url = $"https://www.eldorado.gg/api/offers/me/pauseAllActive/?itemTreeId={_itemId}&offerAttributeIdsCsv={_offerAttributeIds}&tradeEnvironmentId={_tradeEnvironmentId}&offerType={_offerType}";
        }
    }
}
