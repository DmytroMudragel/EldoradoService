using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EldoradoBot.Responses
{
    public class GetActivitiesResponse
    {
        public class Root
        {
            public int undeliveredOrderCount { get; set; }

            public int disputedOrderCount { get; set; }

            public int boostingRequestOfferCount { get; set; }

            public int activeBoostingRequestCount { get; set; }

            public bool hasActiveOrders { get; set; }
        }

    }
}
