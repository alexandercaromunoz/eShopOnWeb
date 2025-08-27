using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ICatalogViewModelService _catalogService;

    public IndexModel(ICatalogViewModelService catalogService)
    {
        _catalogService = catalogService;
    }

    public CatalogIndexViewModel CatalogModel { get; set; } = new();
    public int PageSize { get; set; }

    public async Task OnGet(int? pageId, int? brandFilterApplied, int? typesFilterApplied, int? itemsPage)
    {
        PageSize = itemsPage ?? Constants.ITEMS_PER_PAGE;
        CatalogModel = await _catalogService.GetCatalogItems(pageId ?? 0, PageSize, brandFilterApplied, typesFilterApplied);
    }
}
