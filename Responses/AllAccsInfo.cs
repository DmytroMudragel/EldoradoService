
namespace EldoradoBot.Responses
{
    public class AllAccsInfo
    {
        public class ExchangeRate
        {
            public string? currency { get; set; }
            public double? exchangeRate { get; set; }
        }

        public class ItemHierarchy
        {
            public string? id { get; set; }
            public string? name { get; set; }
            public string? uiJumpToItemTreeId { get; set; }
            public string? seoAlias { get; set; }
        }

        public class MainOfferImage
        {
            public string? smallImage { get; set; }
            public string? largeImage { get; set; }
            public string? originalSizeImage { get; set; }
        }

        public class OfferImage
        {
            public string? smallImage { get; set; }
            public string? largeImage { get; set; }
            public string? originalSizeImage { get; set; }
        }

        public class PricePerUnit
        {
            public double? amount { get; set; }
            public string? currency { get; set; }
        }

        public class Result
        {
            public string? offerTitle { get; set; }
            public MainOfferImage? mainOfferImage { get; set; }
            public List<OfferImage>? offerImages { get; set; }
            public string? id { get; set; }
            public string? userId { get; set; }
            public string? gameId { get; set; }
            public string? itemId { get; set; }
            public string? itemName { get; set; }
            public List<ItemHierarchy>? itemHierarchy { get; set; }
            public string? description { get; set; }
            public int? quantity { get; set; }
            public int? minQuantity { get; set; }
            public List<object>? discounts { get; set; }
            public PricePerUnit? pricePerUnit { get; set; }
            public string? guaranteedDeliveryTime { get; set; }
            public List<TradeEnvironmentValue>? tradeEnvironmentValues { get; set; }
            public List<object>? offerAttributeIdValues { get; set; }
            public DateTime? expireDate { get; set; }
            public string? offerState { get; set; }
            public string? version { get; set; }
            public ExchangeRate? exchangeRate { get; set; }
        }

        public class Root
        {
            public int? pageIndex { get; set; }
            public int? totalPages { get; set; }
            public int? pageSize { get; set; }
            public int? recordCount { get; set; }
            public List<Result>? results { get; set; }
        }

        public class TradeEnvironmentValue
        {
            public string? id { get; set; }
            public string? value { get; set; }
            public string? name { get; set; }
        }
    }
}
