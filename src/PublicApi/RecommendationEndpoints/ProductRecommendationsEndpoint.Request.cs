namespace Microsoft.eShopWeb.PublicApi.RecommendationEndpoints;

public class ProductRecommendationsRequest : BaseRequest
{
    public int CatalogItemId { get; set; }
    public int MaxResults { get; set; } = 4;
}
