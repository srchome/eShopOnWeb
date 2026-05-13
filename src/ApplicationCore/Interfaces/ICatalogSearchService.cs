using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

/// <summary>Provides semantic search and similarity-based recommendations over the product catalog.</summary>
public interface ICatalogSearchService
{
    /// <summary>Returns catalog items ranked by semantic similarity to the natural-language query.</summary>
    Task<IReadOnlyList<CatalogItem>> SearchAsync(string query, int maxResults = 10, CancellationToken ct = default);

    /// <summary>Returns catalog items most similar to the given product, excluding the product itself.</summary>
    Task<IReadOnlyList<CatalogItem>> GetRecommendationsAsync(int catalogItemId, int maxResults = 4, CancellationToken ct = default);
}
