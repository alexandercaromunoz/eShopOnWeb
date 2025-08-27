using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Pages.Shared.Components.BasketComponent;

public class Basket : ViewComponent
{
    private readonly IBasketViewModelService _basketService;

    public Basket(IBasketViewModelService basketService)
    {
        _basketService = basketService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int count = 0;
        try
        {
            var id = GetAnnonymousIdFromCookie();
            if (id != null)
            {
                count = await _basketService.CountTotalBasketItems(id);
            }
        }
        catch (InvalidOperationException)
        {
            count = 0;
        }
        var vm = new BasketComponentViewModel { ItemsCount = count };
        return View(vm);
    }

    private string? GetAnnonymousIdFromCookie()
    {
        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
        {
            var id = Request.Cookies[Constants.BASKET_COOKIENAME];
            if (Guid.TryParse(id, out _)) return id;
        }
        return null;
    }
}
