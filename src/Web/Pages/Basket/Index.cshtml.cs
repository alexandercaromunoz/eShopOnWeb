using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Pages.Basket;

namespace Microsoft.eShopWeb.Web.Pages.Basket;

public class IndexModel : PageModel
{
    private readonly IBasketViewModelService _basketService;

    public IndexModel(IBasketViewModelService basketService)
    {
        _basketService = basketService;
    }

    public BasketViewModel BasketModel { get; set; } = new BasketViewModel();

    public async Task OnGet()
    {
        BasketModel = await _basketService.GetOrCreateBasketForUser(GetOrSetBasketCookieAndUserName());
    }

    public async Task<IActionResult> OnPost(int productId, decimal price)
    {
        if (productId <= 0 || price <= 0) return RedirectToPage("/Index");
        var username = GetOrSetBasketCookieAndUserName();
        BasketModel = await _basketService.AddItemToBasket(username, productId, price, 1);
        return RedirectToPage();
    }

    public async Task OnPostUpdate(IEnumerable<BasketItemViewModel> items)
    {
        if (!ModelState.IsValid) return;
        var existing = await _basketService.GetOrCreateBasketForUser(GetOrSetBasketCookieAndUserName());
        var dict = items.ToDictionary(b => b.Id, b => b.Quantity);
        BasketModel = await _basketService.UpdateQuantities(existing.Id, dict);
    }

    private string GetOrSetBasketCookieAndUserName()
    {
        string? userName = null;
        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
        {
            userName = Request.Cookies[Constants.BASKET_COOKIENAME];
            if (!Guid.TryParse(userName, out _)) userName = null;
        }
        if (userName != null) return userName;
        userName = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Today.AddYears(10) };
        Response.Cookies.Append(Constants.BASKET_COOKIENAME, userName, cookieOptions);
        return userName;
    }
}
