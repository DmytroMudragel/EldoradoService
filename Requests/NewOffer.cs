
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EldoradoBot.Requests
{
    public class NewOffer
    {
        public static string _Url = "https://www.eldorado.gg/api/flexibleOffers";


        public class ExchangeRate
        {
            [JsonPropertyName("currency")]
            public string? Currency { get; set; }

            [JsonPropertyName("exchangeRate")]
            public double Rate { get; set; }
        }

        public class ItemHierarchy
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("uiJumpToItemTreeId")]
            public string? UiJumpToItemTreeId { get; set; }

            [JsonPropertyName("seoAlias")]
            public string? SeoAlias { get; set; }
        }

        public class MainOfferImage
        {
            [JsonPropertyName("smallImage")]
            public string? SmallImage { get; set; }

            [JsonPropertyName("largeImage")]
            public string? LargeImage { get; set; }

            [JsonPropertyName("originalSizeImage")]
            public string? OriginalSizeImage { get; set; }
        }

        public class PricePerUnit
        {
            [JsonPropertyName("amount")]
            public double Amount { get; set; }

            [JsonPropertyName("currency")]
            public string? Currency { get; set; }
        }

        public class Root
        {
            [JsonPropertyName("offerTitle")]
            public string? OfferTitle { get; set; }

            [JsonPropertyName("mainOfferImage")]
            public MainOfferImage? MainOfferImage { get; set; }

            [JsonPropertyName("offerImages")]
            public List<object>? OfferImages { get; set; }

            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("userId")]
            public string? UserId { get; set; }

            [JsonPropertyName("gameId")]
            public string? GameId { get; set; }

            [JsonPropertyName("itemId")]
            public string? ItemId { get; set; }

            [JsonPropertyName("itemName")]
            public string? ItemName { get; set; }

            [JsonPropertyName("itemHierarchy")]
            public List<ItemHierarchy>? ItemHierarchy { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("quantity")]
            public int Quantity { get; set; }

            [JsonPropertyName("minQuantity")]
            public int MinQuantity { get; set; }

            [JsonPropertyName("discounts")]
            public List<object>? Discounts { get; set; }

            [JsonPropertyName("pricePerUnit")]
            public PricePerUnit? PricePerUnit { get; set; }

            [JsonPropertyName("guaranteedDeliveryTime")]
            public string? GuaranteedDeliveryTime { get; set; }

            [JsonPropertyName("tradeEnvironmentValues")]
            public List<TradeEnvironmentValue>? TradeEnvironmentValues { get; set; }

            [JsonPropertyName("offerAttributeIdValues")]
            public List<object>? OfferAttributeIdValues { get; set; }

            [JsonPropertyName("expireDate")]
            public DateTime ExpireDate { get; set; }

            [JsonPropertyName("offerState")]
            public string? OfferState { get; set; }

            [JsonPropertyName("version")]
            public string? Version { get; set; }

            [JsonPropertyName("exchangeRate")]
            public ExchangeRate? ExchangeRate { get; set; }

            public class TradeEnvironmentValue
            {
                [JsonPropertyName("valueId")]
                public string? ValueId { get; set; }

                [JsonPropertyName("value")]
                public string? Value { get; set; }

                [JsonPropertyName("name")]
                public string? Name { get; set; }
            }
        }
    }
}
