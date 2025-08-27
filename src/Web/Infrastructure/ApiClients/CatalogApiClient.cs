using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;
using BlazorShared;

namespace Microsoft.eShopWeb.Web.Infrastructure.ApiClients;

public class CatalogApiClient : ICatalogViewModelService
{
    private readonly HttpClient _httpClient;
    private readonly BaseUrlConfiguration _urls;
    private readonly ILogger<CatalogApiClient> _logger;

    public CatalogApiClient(HttpClient httpClient, IOptions<BaseUrlConfiguration> urls, ILogger<CatalogApiClient> logger)
    {
        _httpClient = httpClient;
        _urls = urls.Value;
        _httpClient.BaseAddress = new Uri(_urls.ApiBase);
        _logger = logger;
    }

    public async Task<CatalogIndexViewModel> GetCatalogItems(int pageIndex, int itemsPage, int? brandId, int? typeId)
    {
        try
        {
            var query = new List<string>();
            if (itemsPage > 0) query.Add($"pageSize={itemsPage}");
            if (pageIndex > 0) query.Add($"pageIndex={pageIndex}");
            if (brandId.HasValue && brandId.Value > 0) query.Add($"catalogBrandId={brandId.Value}");
            if (typeId.HasValue && typeId.Value > 0) query.Add($"catalogTypeId={typeId.Value}");
            var url = "catalog-items" + (query.Count > 0 ? ("?" + string.Join("&", query)) : string.Empty);

            var apiResponse = await _httpClient.GetFromJsonAsync<ListPagedCatalogItemResponse>(url) ?? new();
            var vm = new CatalogIndexViewModel
            {
                CatalogItems = apiResponse.CatalogItems.Select(ci => new CatalogItemViewModel
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    PictureUri = ci.PictureUri,
                    Price = ci.Price
                }).ToList(),
                BrandFilterApplied = brandId ?? 0,
                TypesFilterApplied = typeId ?? 0,
                PaginationInfo = new PaginationInfoViewModel
                {
                    ActualPage = pageIndex,
                    ItemsPerPage = itemsPage,
                    TotalItems = apiResponse.TotalItems,
                    TotalPages = apiResponse.PageCount
                }
            };
            vm.Brands = (await GetBrands(brandId)).ToList();
            vm.Types = (await GetTypes(typeId)).ToList();
            return vm;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Catalog API unreachable at {Base}", _httpClient.BaseAddress);
            return new CatalogIndexViewModel();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Catalog API timeout at {Base}", _httpClient.BaseAddress);
            return new CatalogIndexViewModel();
        }
    }

    // Interface-required parameterless wrappers
    public Task<IEnumerable<SelectListItem>> GetBrands() => GetBrands(null);
    public Task<IEnumerable<SelectListItem>> GetTypes() => GetTypes(null);

    public async Task<IEnumerable<SelectListItem>> GetBrands(int? selectedBrandId = null)
    {
        try
        {
            var list = new List<SelectListItem>();

            // Try wrapped shape first
            ListCatalogBrandsResponse? wrapped = null;
            try { wrapped = await _httpClient.GetFromJsonAsync<ListCatalogBrandsResponse>("catalog-brands"); } catch (System.Text.Json.JsonException) { }

            List<CatalogBrandDto>? flat = null;
            if (wrapped == null || wrapped.CatalogBrands.Count == 0)
            {
                // fallback to plain array
                try { flat = await _httpClient.GetFromJsonAsync<List<CatalogBrandDto>>("catalog-brands"); } catch (System.Text.Json.JsonException) { }
            }
            var brands = wrapped?.CatalogBrands ?? flat ?? new();

            list.Add(new SelectListItem { Text = "All", Value = "", Selected = !(selectedBrandId.HasValue && selectedBrandId.Value > 0) });
            list.AddRange(brands.OrderBy(b => b.DisplayName).Select(b => new SelectListItem
            {
                Text = b.DisplayName,
                Value = b.Id.ToString(),
                Selected = selectedBrandId == b.Id
            }));
            return list;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Brands endpoint unreachable");
            return new List<SelectListItem> { new() { Text = "All", Value = "", Selected = true } };
        }
    }

    public async Task<IEnumerable<SelectListItem>> GetTypes(int? selectedTypeId = null)
    {
        try
        {
            var list = new List<SelectListItem>();

            ListCatalogTypesResponse? wrapped = null;
            try { wrapped = await _httpClient.GetFromJsonAsync<ListCatalogTypesResponse>("catalog-types"); } catch (System.Text.Json.JsonException) { }
            List<CatalogTypeDto>? flat = null;
            if (wrapped == null || wrapped.CatalogTypes.Count == 0)
            {
                try { flat = await _httpClient.GetFromJsonAsync<List<CatalogTypeDto>>("catalog-types"); } catch (System.Text.Json.JsonException) { }
            }
            var types = wrapped?.CatalogTypes ?? flat ?? new();

            list.Add(new SelectListItem { Text = "All", Value = "", Selected = !(selectedTypeId.HasValue && selectedTypeId.Value > 0) });
            list.AddRange(types.OrderBy(t => t.DisplayName).Select(t => new SelectListItem
            {
                Text = t.DisplayName,
                Value = t.Id.ToString(),
                Selected = selectedTypeId == t.Id
            }));
            return list;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Types endpoint unreachable");
            return new List<SelectListItem> { new() { Text = "All", Value = "", Selected = true } };
        }
    }

    private class ListPagedCatalogItemResponse
    {
        public List<CatalogItemDto> CatalogItems { get; set; } = new();
        public int PageCount { get; set; }
        public int TotalItems { get; set; }
    }
    private class CatalogItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PictureUri { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
    // Wrapper responses (PublicApi style)
    private class ListCatalogBrandsResponse { public List<CatalogBrandDto> CatalogBrands { get; set; } = new(); }
    private class ListCatalogTypesResponse { public List<CatalogTypeDto> CatalogTypes { get; set; } = new(); }

    // DTOs support both Brand/Name and Type/Name property names
    private class CatalogBrandDto
    {
        public int Id { get; set; }
        public string? Brand { get; set; } // original
        public string? Name { get; set; }  // alternate
        public string DisplayName => Brand ?? Name ?? string.Empty;
    }
    private class CatalogTypeDto
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string DisplayName => Type ?? Name ?? string.Empty;
    }
}
