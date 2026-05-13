using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.eShopWeb.Web.Extensions;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.eShopWeb.Web.Services;

public class CachedCatalogViewModelService : ICatalogViewModelService
{
    private readonly IDistributedCache _cache;
    private readonly CatalogViewModelService _catalogViewModelService;

    public CachedCatalogViewModelService(IDistributedCache cache,
        CatalogViewModelService catalogViewModelService)
    {
        _cache = cache;
        _catalogViewModelService = catalogViewModelService;
    }

    public async Task<IEnumerable<SelectListItem>> GetBrands()
    {
        return await GetOrCreateAsync(
            CacheHelpers.GenerateBrandsCacheKey(),
            () => _catalogViewModelService.GetBrands(),
            new List<SelectListItem>());
    }

    public async Task<CatalogIndexViewModel> GetCatalogItems(int pageIndex, int itemsPage, int? brandId, int? typeId)
    {
        var cacheKey = CacheHelpers.GenerateCatalogItemCacheKey(pageIndex, Constants.ITEMS_PER_PAGE, brandId, typeId);

        return await GetOrCreateAsync(
            cacheKey,
            () => _catalogViewModelService.GetCatalogItems(pageIndex, itemsPage, brandId, typeId),
            new CatalogIndexViewModel());
    }

    public async Task<IEnumerable<SelectListItem>> GetTypes()
    {
        return await GetOrCreateAsync(
            CacheHelpers.GenerateTypesCacheKey(),
            () => _catalogViewModelService.GetTypes(),
            new List<SelectListItem>());
    }

    private async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, T defaultValue)
    {
        var cached = await _cache.GetStringAsync(key);
        if (cached != null)
        {
            return JsonSerializer.Deserialize<T>(cached) ?? defaultValue;
        }

        var value = await factory();
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = CacheHelpers.DefaultCacheDuration
        };
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
        return value;
    }
}
