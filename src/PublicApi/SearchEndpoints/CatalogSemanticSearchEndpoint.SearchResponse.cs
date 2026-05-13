using System;
using System.Collections.Generic;
using Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

namespace Microsoft.eShopWeb.PublicApi.SearchEndpoints;

public class CatalogSemanticSearchResponse : BaseResponse
{
    public CatalogSemanticSearchResponse(Guid correlationId) : base(correlationId) { }
    public CatalogSemanticSearchResponse() { }

    public List<CatalogItemDto> CatalogItems { get; set; } = new();
}
