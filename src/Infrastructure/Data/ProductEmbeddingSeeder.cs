using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Data;

public static class ProductEmbeddingSeeder
{
    public static async Task SeedAsync(CatalogContext context, IEmbeddingService embeddingService, ILogger logger)
    {
        var items = await context.CatalogItems
            .Include(i => i.CatalogBrand)
            .Include(i => i.CatalogType)
            .ToListAsync();

        var existingIds = await context.ProductEmbeddings
            .Select(e => e.CatalogItemId)
            .ToListAsync();

        var missing = items.Where(i => !existingIds.Contains(i.Id)).ToList();

        if (missing.Count == 0) return;

        logger.LogInformation("Generating embeddings for {Count} catalog items...", missing.Count);

        foreach (var item in missing)
        {
            try
            {
                var text = $"Product: {item.Name}. {item.Description}. Brand: {item.CatalogBrand?.Brand}. Type: {item.CatalogType?.Type}.";
                var embedding = await embeddingService.GenerateEmbeddingAsync(text);
                context.ProductEmbeddings.Add(new ProductEmbedding(item.Id, embedding));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to generate embedding for catalog item {Id} ({Name})", item.Id, item.Name);
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Embeddings seeded for {Count} products.", missing.Count);
    }
}
