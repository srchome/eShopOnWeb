using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.Infrastructure.Services;

public class CatalogSearchServiceTests
{
    private static CatalogContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new CatalogContext(options);
    }

    [Fact]
    public async Task SearchAsync_ReturnsItemsRankedBySimilarity()
    {
        using var context = CreateInMemoryContext(nameof(SearchAsync_ReturnsItemsRankedBySimilarity));

        var brand = new CatalogBrand(".NET");
        var type = new CatalogType("T-Shirt");
        context.CatalogBrands.Add(brand);
        context.CatalogTypes.Add(type);
        var item1 = new CatalogItem(type.Id, brand.Id, "Blue shirt", "Blue shirt", 10m, "img1.png");
        var item2 = new CatalogItem(type.Id, brand.Id, "Red mug", "Red mug", 8m, "img2.png");
        context.CatalogItems.AddRange(item1, item2);
        await context.SaveChangesAsync();

        // item1 has cosine similarity 1.0 with query; item2 has ~0.8 — both above threshold
        var blueShirtEmbedding = new float[] { 1f, 0f, 0f };
        var redMugEmbedding = new float[] { 0.8f, 0.6f, 0f };
        var queryEmbedding = new float[] { 1f, 0f, 0f };

        context.ProductEmbeddings.Add(new ProductEmbedding(item1.Id, blueShirtEmbedding));
        context.ProductEmbeddings.Add(new ProductEmbedding(item2.Id, redMugEmbedding));
        await context.SaveChangesAsync();

        var mockEmbeddingService = Substitute.For<IEmbeddingService>();
        mockEmbeddingService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(queryEmbedding);

        var service = new CatalogSearchService(context, mockEmbeddingService, NullLogger<CatalogSearchService>.Instance);

        var results = await service.SearchAsync("blue shirt");

        Assert.Equal(2, results.Count);
        Assert.Equal(item1.Id, results[0].Id);
        Assert.Equal(item2.Id, results[1].Id);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ExcludesSourceItem()
    {
        using var context = CreateInMemoryContext(nameof(GetRecommendationsAsync_ExcludesSourceItem));

        var brand = new CatalogBrand(".NET");
        var type = new CatalogType("Mug");
        context.CatalogBrands.Add(brand);
        context.CatalogTypes.Add(type);
        var source = new CatalogItem(type.Id, brand.Id, "Source Item", "Source", 10m, "s.png");
        var similar = new CatalogItem(type.Id, brand.Id, "Similar Item", "Similar", 10m, "sim.png");
        context.CatalogItems.AddRange(source, similar);
        await context.SaveChangesAsync();

        context.ProductEmbeddings.Add(new ProductEmbedding(source.Id, new float[] { 1f, 0f }));
        context.ProductEmbeddings.Add(new ProductEmbedding(similar.Id, new float[] { 0.9f, 0.1f }));
        await context.SaveChangesAsync();

        var mockEmbeddingService = Substitute.For<IEmbeddingService>();
        var service = new CatalogSearchService(context, mockEmbeddingService, NullLogger<CatalogSearchService>.Instance);

        var recommendations = await service.GetRecommendationsAsync(source.Id, maxResults: 4);

        Assert.DoesNotContain(recommendations, r => r.Id == source.Id);
        Assert.Single(recommendations);
        Assert.Equal(similar.Id, recommendations[0].Id);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ReturnsEmpty_WhenNoEmbeddingExists()
    {
        using var context = CreateInMemoryContext(nameof(GetRecommendationsAsync_ReturnsEmpty_WhenNoEmbeddingExists));

        var mockEmbeddingService = Substitute.For<IEmbeddingService>();
        var service = new CatalogSearchService(context, mockEmbeddingService, NullLogger<CatalogSearchService>.Instance);

        var result = await service.GetRecommendationsAsync(999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_RespectsMaxResults()
    {
        using var context = CreateInMemoryContext(nameof(SearchAsync_RespectsMaxResults));

        var brand = new CatalogBrand("Brand");
        var type = new CatalogType("Type");
        context.CatalogBrands.Add(brand);
        context.CatalogTypes.Add(type);

        for (int i = 0; i < 5; i++)
        {
            var item = new CatalogItem(type.Id, brand.Id, $"Item {i}", $"Desc {i}", 10m, $"img{i}.png");
            context.CatalogItems.Add(item);
            await context.SaveChangesAsync();
            context.ProductEmbeddings.Add(new ProductEmbedding(item.Id, new float[] { 1f, (float)i * 0.1f }));
        }
        await context.SaveChangesAsync();

        var mockEmbeddingService = Substitute.For<IEmbeddingService>();
        mockEmbeddingService.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new float[] { 1f, 0f });

        var service = new CatalogSearchService(context, mockEmbeddingService, NullLogger<CatalogSearchService>.Instance);

        var results = await service.SearchAsync("query", maxResults: 3);

        Assert.Equal(3, results.Count);
    }
}
