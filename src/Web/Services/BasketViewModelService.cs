using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Pages.Basket;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.Web.Services;

public class BasketViewModelService : IBasketViewModelService
{
    private readonly IRepository<Basket> _basketRepository;
    private readonly IReadRepository<CatalogItem> _catalogItemRepository;
    private readonly IBasketQueryService _basketQueryService;
    private readonly IUriComposer _uriComposer;

    public BasketViewModelService(IRepository<Basket> basketRepository,
        IReadRepository<CatalogItem> catalogItemRepository,
        IBasketQueryService basketQueryService,
        IUriComposer uriComposer)
    {
        _basketRepository = basketRepository;
        _catalogItemRepository = catalogItemRepository;
        _basketQueryService = basketQueryService;
        _uriComposer = uriComposer;
    }

    public async Task<BasketViewModel> GetOrCreateBasketForUser(string userName)
    {
        var spec = new BasketWithItemsSpecification(userName);
        var basket = await _basketRepository.FirstOrDefaultAsync(spec);
        if (basket == null)
        {
            basket = new Basket(userName);
            await _basketRepository.AddAsync(basket);
        }
        return await MapAsync(basket);
    }

    public Task<int> CountTotalBasketItems(string userName) => _basketQueryService.CountTotalBasketItems(userName);

    public async Task<BasketViewModel> AddItemToBasket(string userName, int catalogItemId, decimal price, int quantity = 1)
    {
        var spec = new BasketWithItemsSpecification(userName);
        var basket = await _basketRepository.FirstOrDefaultAsync(spec);
        if (basket == null)
        {
            basket = new Basket(userName);
            await _basketRepository.AddAsync(basket);
        }
        basket.AddItem(catalogItemId, price, quantity);
        await _basketRepository.UpdateAsync(basket);
        return await MapAsync(basket);
    }

    public async Task<BasketViewModel> UpdateQuantities(int basketId, Dictionary<int, int> quantities)
    {
        var spec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.FirstOrDefaultAsync(spec);
        if (basket == null) return new BasketViewModel();
        foreach (var item in basket.Items)
        {
            if (quantities.TryGetValue(item.Id, out var q)) item.SetQuantity(q);
        }
        basket.RemoveEmptyItems();
        await _basketRepository.UpdateAsync(basket);
        return await MapAsync(basket);
    }

    private async Task<BasketViewModel> MapAsync(Basket basket)
    {
        var vm = new BasketViewModel { Id = basket.Id, BuyerId = basket.BuyerId };
        if (!basket.Items.Any()) return vm;

        var catalogIds = basket.Items.Select(i => i.CatalogItemId).Distinct().ToArray();
        var catalogItems = await _catalogItemRepository.ListAsync(new CatalogItemsSpecification(catalogIds));
        var lookup = catalogItems.ToDictionary(ci => ci.Id, ci => ci);

        foreach (var item in basket.Items)
        {
            lookup.TryGetValue(item.CatalogItemId, out var cat);
            var pic = cat?.PictureUri ?? string.Empty;
            if (!string.IsNullOrEmpty(pic)) pic = _uriComposer.ComposePicUri(pic);
            vm.Items.Add(new BasketItemViewModel
            {
                Id = item.Id,
                CatalogItemId = item.CatalogItemId,
                // Use description (or name) for display instead of numeric id
                ProductName = !string.IsNullOrEmpty(cat?.Description) ? cat!.Description : (cat?.Name ?? item.CatalogItemId.ToString()),
                UnitPrice = item.UnitPrice,
                OldUnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                PictureUrl = pic
            });
        }
        return vm;
    }
}
