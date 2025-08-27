using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Interfaces; // for services
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate; // for Address

namespace Microsoft.eShopWeb.Web.Pages.Basket;

[Authorize]
public class CheckoutModel : PageModel
{
    private readonly IBasketViewModelService _basketService;
    private readonly IBasketService _basketDomainService;
    private readonly IOrderService _orderService; // new
    private string? _username = null;

    public CheckoutModel(IBasketViewModelService basketService, IBasketService basketDomainService, IOrderService orderService)
    {
        _basketService = basketService;
        _basketDomainService = basketDomainService;
        _orderService = orderService;
    }

    public BasketViewModel BasketModel { get; set; } = new BasketViewModel();

    public async Task OnGet()
    {
        await SetBasketModelAsync();
    }

    public async Task<IActionResult> OnPost(IEnumerable<BasketItemViewModel> items, string Street, string City, string State, string Country, string ZipCode)
    {
        await SetBasketModelAsync();
        if (!ModelState.IsValid) return BadRequest();
        var updateModel = items.ToDictionary(b => b.Id, b => b.Quantity);
        BasketModel = await _basketService.UpdateQuantities(BasketModel.Id, updateModel);

        // Create order from basket BEFORE clearing basket
        if (BasketModel.Items.Any())
        {
            // Placeholder shipping; could be extended with user input form
            var address = new Address(Street ?? string.Empty, City ?? string.Empty, State ?? string.Empty, Country ?? string.Empty, ZipCode ?? string.Empty);
            await _orderService.CreateOrderAsync(BasketModel.Id, address);
        }
        await _basketDomainService.ClearBasketAsync(BasketModel.Id);
        return RedirectToPage("Success");
    }

    private async Task SetBasketModelAsync()
    {
        GetOrSetBasketCookieAndUserName();
        BasketModel = await _basketService.GetOrCreateBasketForUser(_username!);
    }

    private void GetOrSetBasketCookieAndUserName()
    {
        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME)) _username = Request.Cookies[Constants.BASKET_COOKIENAME];
        if (_username != null) return;
        _username = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions { Expires = DateTime.Today.AddYears(10) };
        Response.Cookies.Append(Constants.BASKET_COOKIENAME, _username, cookieOptions);
    }
}
