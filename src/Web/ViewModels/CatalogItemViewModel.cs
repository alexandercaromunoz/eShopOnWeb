namespace Microsoft.eShopWeb.Web.ViewModels;

public class CatalogItemViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? PictureUri { get; set; }
    public decimal Price { get; set; }
}

public class CatalogIndexViewModel
{
    public List<CatalogItemViewModel> CatalogItems { get; set; } = new();
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Brands { get; set; } = new();
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Types { get; set; } = new();
    public int? BrandFilterApplied { get; set; }
    public int? TypesFilterApplied { get; set; }
    public PaginationInfoViewModel? PaginationInfo { get; set; }
}

