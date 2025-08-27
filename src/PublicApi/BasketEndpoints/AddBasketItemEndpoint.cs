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
public class AddBasketItemEndpoint : IEndpoint<IResult, ModifyBasketItemRequest, IRepository<Basket>>
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("api/basket/items", async (ModifyBasketItemRequest request, IRepository<Basket> basketRepo) =>
        {
            return await HandleAsync(request, basketRepo);
        }).Produces<BasketResponse>()
        .WithTags("BasketEndpoints")
        .RequireAuthorization();
    }

    public async Task<IResult> HandleAsync(ModifyBasketItemRequest request, IRepository<Basket> basketRepository)
    {
        var spec = new BasketWithItemsSpecification(request.BuyerId);
        var basket = await basketRepository.FirstOrDefaultAsync(spec);
        if (basket == null)
        {
            basket = new Basket(request.BuyerId);
            await basketRepository.AddAsync(basket);
        }
        basket.AddItem(request.CatalogItemId, request.Price, request.Quantity);
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
