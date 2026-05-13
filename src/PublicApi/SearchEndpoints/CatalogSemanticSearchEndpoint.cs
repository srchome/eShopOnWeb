using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

namespace Microsoft.eShopWeb.PublicApi.SearchEndpoints;

/// <summary>
/// Search catalog items using natural language semantic similarity.
/// </summary>
public class CatalogSemanticSearchEndpoint(ICatalogSearchService searchService, IUriComposer uriComposer, AutoMapper.IMapper mapper)
    : Endpoint<CatalogSemanticSearchRequest, CatalogSemanticSearchResponse>
{
    public override void Configure()
    {
        Post("api/catalog-items/search");
        AllowAnonymous();
        Description(d =>
            d.Produces<CatalogSemanticSearchResponse>()
             .WithTags("SearchEndpoints"));
    }

    public override async Task<CatalogSemanticSearchResponse> ExecuteAsync(CatalogSemanticSearchRequest request, CancellationToken ct)
    {
        var response = new CatalogSemanticSearchResponse(request.CorrelationId());

        var items = await searchService.SearchAsync(request.Query, request.MaxResults, ct);
        response.CatalogItems.AddRange(items.Select(mapper.Map<CatalogItemDto>));
        foreach (var item in response.CatalogItems)
        {
            item.PictureUri = uriComposer.ComposePicUri(item.PictureUri);
        }

        return response;
    }
}
