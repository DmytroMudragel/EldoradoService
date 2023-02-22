using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EldoradoBot.Requests
{
    public class AllOffersInfoRequest
    {
        public string _Url = "https://www.eldorado.gg/api/flexibleOffers/me?pageIndex=1&pageSize=40&offerType=Account";

        public int _pageIndex = 1;

        public int _pageSize = 40;

        public string _offerType = "Account";

        public AllOffersInfoRequest(int pageIndex, int pageSize , string offerType)
        {
            _pageIndex = pageIndex;
            _pageSize = pageSize;
            _offerType = offerType;
            _Url = $"https://www.eldorado.gg/api/flexibleOffers/me?pageIndex={_pageIndex}&pageSize={_pageSize}&offerType={_offerType}";
        }
    }
}
