using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Services;

public class CatalogSearchService : ICatalogSearchService
{
    private readonly CatalogContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<CatalogSearchService> _logger;

    public CatalogSearchService(CatalogContext context, IEmbeddingService embeddingService, ILogger<CatalogSearchService> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    private const float SearchSimilarityThreshold = 0.30f;

    public async Task<IReadOnlyList<CatalogItem>> SearchAsync(string query, int maxResults = 10, CancellationToken ct = default)
    {
        _logger.LogInformation("Semantic search for: {Query}", query);
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, ct);
        return await RankBySimilarity(queryEmbedding, excludeItemId: null, maxResults, minScore: SearchSimilarityThreshold, ct);
    }

    public async Task<IReadOnlyList<CatalogItem>> GetRecommendationsAsync(int catalogItemId, int maxResults = 4, CancellationToken ct = default)
    {
        var sourceEmbedding = await _context.ProductEmbeddings
            .FirstOrDefaultAsync(e => e.CatalogItemId == catalogItemId, ct);

        if (sourceEmbedding is null)
        {
            _logger.LogWarning("No embedding found for catalog item {Id}", catalogItemId);
            return [];
        }

        return await RankBySimilarity(sourceEmbedding.GetEmbedding(), excludeItemId: catalogItemId, maxResults, minScore: 0, ct);
    }

    private async Task<IReadOnlyList<CatalogItem>> RankBySimilarity(
        float[] queryEmbedding, int? excludeItemId, int maxResults, float minScore, CancellationToken ct)
    {
        var allEmbeddings = await _context.ProductEmbeddings.ToListAsync(ct);
        var allItems = await _context.CatalogItems
            .Include(i => i.CatalogBrand)
            .Include(i => i.CatalogType)
            .ToListAsync(ct);

        var itemMap = allItems.ToDictionary(i => i.Id);

        return allEmbeddings
            .Where(e => e.CatalogItemId != excludeItemId && itemMap.ContainsKey(e.CatalogItemId))
            .Select(e => (item: itemMap[e.CatalogItemId], score: CosineSimilarity(queryEmbedding, e.GetEmbedding())))
            .Where(x => x.score >= minScore)
            .OrderByDescending(x => x.score)
            .Take(maxResults)
            .Select(x => x.item)
            .ToList();
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        float magnitude = MathF.Sqrt(magA) * MathF.Sqrt(magB);
        return magnitude == 0 ? 0 : dot / magnitude;
    }
}
