using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EldoradoBot.Requests
{
    internal class DeleteAccFromEldoradoRequest
    {
        public string _Url;

        public string _itemId;

        public DeleteAccFromEldoradoRequest(string itemId)
        {
            _itemId = itemId;
            _Url = $"https://www.eldorado.gg/api/offers/{_itemId}/";
        }
    }
}
