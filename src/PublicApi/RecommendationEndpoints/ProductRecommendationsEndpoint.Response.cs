using System;
using System.Collections.Generic;
using Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

namespace Microsoft.eShopWeb.PublicApi.RecommendationEndpoints;

public class ProductRecommendationsResponse : BaseResponse
{
    public ProductRecommendationsResponse(Guid correlationId) : base(correlationId) { }
    public ProductRecommendationsResponse() { }

    public List<CatalogItemDto> RecommendedItems { get; set; } = new();
}
