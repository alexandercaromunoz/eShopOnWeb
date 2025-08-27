using Microsoft.eShopWeb.Web.Pages.Basket;

namespace Microsoft.eShopWeb.Web.Interfaces;

public interface IBasketViewModelService
{
    Task<BasketViewModel> GetOrCreateBasketForUser(string userName);
    Task<int> CountTotalBasketItems(string userName);
    Task<BasketViewModel> AddItemToBasket(string userName, int catalogItemId, decimal price, int quantity = 1);
    Task<BasketViewModel> UpdateQuantities(int basketId, Dictionary<int,int> quantities);
}
