# Implementation Plan: AI-Powered Product Search & Recommendations

## Context

The eShopOnWeb catalog currently supports only brand/type filter-based product discovery. Customers can't find products by intent (e.g. "cozy .NET gear for winter") and there are no cross-sell recommendations. This plan adds two features using OpenAI embeddings:

1. **Semantic Search** — natural language query on the catalog page returns products ranked by semantic similarity
2. **Recommendations** — "You might also like" widget on the catalog page based on the top search result

The user has a standard OpenAI API key in `.env` as `OpenAI__ApiKey`. Embeddings are stored in SQL Server (existing Docker infrastructure) as JSON float arrays. Similarity is computed in-memory (12 products — trivially fast). No new infrastructure is required.

---

## Tech Stack Decisions

| Concern | Decision | Reason |
|---|---|---|
| OpenAI SDK | `OpenAI` NuGet package v2.x | Official, supports `text-embedding-3-small`, no Azure dependency |
| Embedding model | `text-embedding-3-small` (1536 dims) | Cheap, fast, high quality |
| Embedding storage | New `ProductEmbeddings` SQL Server table, `float[]` as JSON NVARCHAR(MAX) | No new infra, existing EF migrations |
| Similarity search | In-memory cosine similarity | 12 products — no vector DB needed |
| UI trigger | Server-side Razor Pages form (`?q=`) | Consistent with existing MVC Razor Pages style |
| Recommendations placement | Bottom of catalog index page, populated from top search result | No new page needed |

---

## Configuration

`.env` already has:
- `OpenAI__ApiKey` — API key (already set)
- `OpenAI__Model` — can use for embedding model name

Add to `src/PublicApi/appsettings.json` and `src/Web/appsettings.json`:
```json
"OpenAI": {
  "EmbeddingModel": "text-embedding-3-small"
}
```

New `OpenAISettings` POCO (ApplicationCore) reads both `ApiKey` and `EmbeddingModel` from config.

---

## Sprint Breakdown

### Sprint 1: Foundation — Entities, Config, EF (Day 1) ✅

**New files:**

1. `src/ApplicationCore/Settings/OpenAISettings.cs`
   ```csharp
   public class OpenAISettings
   {
       public string ApiKey { get; set; } = string.Empty;
       public string EmbeddingModel { get; set; } = "text-embedding-3-small";
   }
   ```

2. `src/ApplicationCore/Entities/ProductEmbedding.cs`
   ```csharp
   public class ProductEmbedding : BaseEntity, IAggregateRoot
   {
       public int CatalogItemId { get; private set; }
       public string EmbeddingJson { get; private set; }  // JSON float[]
       public DateTime CreatedAt { get; private set; }

       public ProductEmbedding(int catalogItemId, float[] embedding)
       {
           CatalogItemId = catalogItemId;
           EmbeddingJson = JsonSerializer.Serialize(embedding);
           CreatedAt = DateTime.UtcNow;
       }

       public float[] GetEmbedding() => JsonSerializer.Deserialize<float[]>(EmbeddingJson)!;
   }
   ```

3. `src/Infrastructure/Data/Config/ProductEmbeddingConfiguration.cs`
   — `IEntityTypeConfiguration<ProductEmbedding>`, maps to `ProductEmbeddings` table, `EmbeddingJson` as `NVARCHAR(MAX)`

**Modified files:**

4. `src/Infrastructure/Data/CatalogContext.cs` — add `DbSet<ProductEmbedding> ProductEmbeddings`
5. `Directory.Packages.props` — add `<PackageVersion Include="OpenAI" Version="2.2.0" />`
6. `src/Infrastructure/Infrastructure.csproj` — add `<PackageReference Include="OpenAI" />`

**Generated:**

7. EF Core migration: `dotnet ef migrations add AddProductEmbeddings -p src/Infrastructure -s src/PublicApi --context CatalogContext`

---

### Sprint 2: Embedding Service & Seeding (Day 2) ✅

**New files:**

8. `src/ApplicationCore/Interfaces/IEmbeddingService.cs`
   ```csharp
   public interface IEmbeddingService
   {
       Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
   }
   ```

9. `src/Infrastructure/Services/OpenAIEmbeddingService.cs`
   — Implements `IEmbeddingService`. Uses `OpenAI.OpenAIClient` with `EmbeddingClient`.
   — Client is lazy-initialized in `GenerateEmbeddingAsync` (constructor does not throw when key is empty).
   — Builds product text as `$"Product: {name}. {description}. Brand: {brand}. Type: {type}."`.

10. `src/Infrastructure/Data/ProductEmbeddingSeeder.cs`
    — Static class, idempotent (skips items that already have embeddings), graceful per-item error handling.

11. `src/ApplicationCore/Interfaces/ICatalogSearchService.cs`
    ```csharp
    public interface ICatalogSearchService
    {
        Task<IReadOnlyList<CatalogItem>> SearchAsync(string query, int maxResults = 10, CancellationToken ct = default);
        Task<IReadOnlyList<CatalogItem>> GetRecommendationsAsync(int catalogItemId, int maxResults = 4, CancellationToken ct = default);
    }
    ```

12. `src/Infrastructure/Services/CatalogSearchService.cs`
    — Loads all embeddings + catalog items from DB, generates embedding for query, computes cosine similarity in memory, returns ranked `CatalogItem` list.
    — Cosine similarity: `dot / (sqrt(magA) * sqrt(magB))`

**Modified files:**

13. `src/PublicApi/Extensions/ServiceCollectionExtensions.cs` — register `OpenAIEmbeddingService` and `CatalogSearchService`, bind `OpenAISettings`
14. `src/Web/Configuration/ConfigureCoreServices.cs` — same registrations
15. `src/PublicApi/Extensions/WebApplicationExtensions.cs` — call seeder after identity seed, guarded by `OpenAISettings.ApiKey` check
16. `src/Web/Extensions/WebApplicationExtensions.cs` — same seeder call with same guard

> **Note:** Both `WebApplicationExtensions` check `OpenAISettings.ApiKey` before resolving `IEmbeddingService` and calling the seeder. This allows the app to start cleanly in environments without an API key (e.g. integration tests, CI).

---

### Sprint 3: PublicApi Endpoints (Day 3) ✅

**New files (follow FastEndpoints conventions exactly):**

17. `src/PublicApi/SearchEndpoints/CatalogSemanticSearchEndpoint.cs`
    - `POST api/catalog-items/search`
    - `AllowAnonymous()`
    - Body: `{ "query": "cozy .NET gear", "maxResults": 10 }`
    - Returns `CatalogItemDto[]` ranked by similarity

18. `src/PublicApi/SearchEndpoints/CatalogSemanticSearchEndpoint.SearchRequest.cs`
19. `src/PublicApi/SearchEndpoints/CatalogSemanticSearchEndpoint.SearchResponse.cs`

20. `src/PublicApi/RecommendationEndpoints/ProductRecommendationsEndpoint.cs`
    - `GET api/catalog-items/{catalogItemId}/recommendations`
    - `AllowAnonymous()`
    - Returns `NotFound` when no embedding exists for the item

21. `src/PublicApi/RecommendationEndpoints/ProductRecommendationsEndpoint.Request.cs`
22. `src/PublicApi/RecommendationEndpoints/ProductRecommendationsEndpoint.Response.cs`

Both endpoints reuse existing `CatalogItemDto` and `IUriComposer` (for picture URIs).

---

### Sprint 4: Web UI (Day 4) ✅

**Modified files:**

23. `src/Web/Pages/Index.cshtml.cs`
    - Inject `ICatalogSearchService` and `IUriComposer`
    - `OnGet` handles `[FromQuery(Name = "q")] string? searchQuery`
    - If query: call `SearchAsync`, map results, call `GetRecommendationsAsync` for top result
    - If no query: standard `GetCatalogItems` behavior

24. `src/Web/ViewModels/CatalogIndexViewModel.cs`
    - Add `public List<CatalogItemViewModel> RecommendedItems { get; set; } = new()`
    - Add `public string? SearchQuery { get; set; }`

25. `src/Web/Pages/Index.cshtml`
    - Search form at top (`name="q"`, text input + submit button)
    - "Clear search" link when `SearchQuery != null`
    - Brand/type filter hidden during active search
    - Conditional pagination (null-checked)
    - `_recommendations` partial rendered at bottom

**New file:**

26. `src/Web/Pages/Shared/_recommendations.cshtml`
    - Renders "You might also like" heading + `_product.cshtml` partials in a `col-md-3` grid

---

### Sprint 5: Unit Tests (Day 5) ✅

27. `tests/UnitTests/Infrastructure/Services/CatalogSearchServiceTests.cs`
    - Uses EF InMemory database + NSubstitute mock for `IEmbeddingService`
    - `SearchAsync_ReturnsItemsRankedBySimilarity` — higher-similarity item ranks first
    - `GetRecommendationsAsync_ExcludesSourceItem` — source item not in results
    - `GetRecommendationsAsync_ReturnsEmpty_WhenNoEmbeddingExists` — missing embedding returns empty
    - `SearchAsync_RespectsMaxResults` — result count capped at maxResults

---

## Verification Results

| Check | Result |
|---|---|
| `dotnet build eShopOnWeb.sln` | 0 errors |
| EF migration `AddProductEmbeddings` | Created and applied |
| Unit tests (52 total) | All pass |
| Integration tests (54 total) | All pass |
| `POST /api/catalog-items/search` with `{ "query": "black .NET sweatshirt" }` | Returns ranked results |
| `GET /api/catalog-items/1/recommendations` | Returns similar items excluding item 1 |
| `http://localhost:5106/?q=blue+.NET+shirt` | Search results + "You might also like" renders |
| Docker rebuild (`docker compose build --no-cache && docker compose up -d`) | All 3 containers healthy |

---

## Estimated Effort

| Sprint | Effort |
|---|---|
| Sprint 1: Foundation | 4-6 hours |
| Sprint 2: Embedding service + seeding | 4-6 hours |
| Sprint 3: API endpoints | 3-4 hours |
| Sprint 4: Web UI | 3-4 hours |
| Sprint 5: Tests | 2-3 hours |
| **Total** | **~2 working days** |
