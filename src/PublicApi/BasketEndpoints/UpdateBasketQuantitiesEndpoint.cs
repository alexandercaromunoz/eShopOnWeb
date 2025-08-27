using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.BasketEndpoints;

[Authorize]
public class UpdateBasketQuantitiesEndpoint : IEndpoint<IResult, UpdateBasketQuantitiesRequest, IRepository<Basket>>
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPut("api/basket", async (UpdateBasketQuantitiesRequest request, IRepository<Basket> basketRepo) =>
        {
            return await HandleAsync(request, basketRepo);
        }).Produces<BasketResponse>()
        .WithTags("BasketEndpoints")
        .RequireAuthorization();
    }

    public async Task<IResult> HandleAsync(UpdateBasketQuantitiesRequest request, IRepository<Basket> basketRepository)
    {
        var basket = await basketRepository.GetByIdAsync(request.BasketId);
        if (basket == null)
        {
            return Results.NotFound();
        }
        foreach (var kvp in request.Quantities)
        {
            var item = basket.Items.FirstOrDefault(i => i.Id == kvp.Key);
            if (item != null)
            {
                item.SetQuantity(kvp.Value); // BasketItem has SetQuantity
            }
        }
        basket.RemoveEmptyItems();
        await basketRepository.UpdateAsync(basket);
        return Results.Ok(new BasketResponse(request.CorrelationId())
        {
            Basket = new BasketDto
            {
                Id = basket.Id,
                BuyerId = basket.BuyerId,
                Items = basket.Items.Select(i => new BasketItemDto
                {
                    Id = i.Id,
                    CatalogItemId = i.CatalogItemId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            }
        });
    }
}
