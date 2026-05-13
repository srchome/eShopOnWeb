using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

namespace Microsoft.eShopWeb.PublicApi.RecommendationEndpoints;

/// <summary>
/// Returns products similar to the given catalog item.
/// </summary>
public class ProductRecommendationsEndpoint(ICatalogSearchService searchService, IUriComposer uriComposer, AutoMapper.IMapper mapper)
    : Endpoint<ProductRecommendationsRequest, Results<Ok<ProductRecommendationsResponse>, NotFound>>
{
    public override void Configure()
    {
        Get("api/catalog-items/{catalogItemId}/recommendations");
        AllowAnonymous();
        Description(d =>
            d.Produces<ProductRecommendationsResponse>()
             .WithTags("RecommendationEndpoints"));
    }

    public override async Task<Results<Ok<ProductRecommendationsResponse>, NotFound>> ExecuteAsync(
        ProductRecommendationsRequest request, CancellationToken ct)
    {
        var items = await searchService.GetRecommendationsAsync(request.CatalogItemId, request.MaxResults, ct);

        if (!items.Any())
            return TypedResults.NotFound();

        var response = new ProductRecommendationsResponse(request.CorrelationId());
        response.RecommendedItems.AddRange(items.Select(mapper.Map<CatalogItemDto>));
        foreach (var item in response.RecommendedItems)
        {
            item.PictureUri = uriComposer.ComposePicUri(item.PictureUri);
        }

        return TypedResults.Ok(response);
    }
}
