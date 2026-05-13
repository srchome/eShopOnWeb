namespace Microsoft.eShopWeb.PublicApi.SearchEndpoints;

public class CatalogSemanticSearchRequest : BaseRequest
{
    public string Query { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 10;
}
