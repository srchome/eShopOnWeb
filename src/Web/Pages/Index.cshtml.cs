using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ICatalogViewModelService _catalogViewModelService;
    private readonly ICatalogSearchService _catalogSearchService;
    private readonly IUriComposer _uriComposer;

    public IndexModel(
        ICatalogViewModelService catalogViewModelService,
        ICatalogSearchService catalogSearchService,
        IUriComposer uriComposer)
    {
        _catalogViewModelService = catalogViewModelService;
        _catalogSearchService = catalogSearchService;
        _uriComposer = uriComposer;
    }

    public required CatalogIndexViewModel CatalogModel { get; set; } = new CatalogIndexViewModel();

    public async Task OnGet(int? pageId, [FromQuery(Name = "q")] string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var allResults = await _catalogSearchService.SearchAsync(searchQuery, maxResults: 60);
            var currentPage = pageId ?? 0;
            var pageSize = Constants.ITEMS_PER_PAGE;
            var totalResults = allResults.Count;
            var totalPages = (int)Math.Ceiling((double)totalResults / pageSize);

            CatalogModel = new CatalogIndexViewModel();
            CatalogModel.SearchQuery = searchQuery;
            CatalogModel.CatalogItems = allResults
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .Select(ToViewModel)
                .ToList();

            CatalogModel.PaginationInfo = totalPages > 1
                ? new PaginationInfoViewModel
                {
                    ActualPage = currentPage,
                    ItemsPerPage = pageSize,
                    TotalItems = totalResults,
                    TotalPages = totalPages,
                    HasPreviousPage = currentPage > 0,
                    HasNextPage = currentPage < totalPages - 1
                }
                : null;

            if (allResults.Any())
            {
                var topItemId = allResults[0].Id;
                var recommended = await _catalogSearchService.GetRecommendationsAsync(topItemId, maxResults: 4);
                CatalogModel.RecommendedItems = recommended.Select(ToViewModel).ToList();
            }
        }
        else
        {
            CatalogModel = await _catalogViewModelService.GetCatalogItems(
                pageId ?? 0, Constants.ITEMS_PER_PAGE, null, null);
        }
    }

    private CatalogItemViewModel ToViewModel(CatalogItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        PictureUri = _uriComposer.ComposePicUri(item.PictureUri),
        Price = item.Price
    };
}
