using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.BasketEndpoints;

[Authorize]
public class GetBasketEndpoint : IEndpoint<IResult, GetBasketRequest, IRepository<Basket>>
{
    private readonly IUriComposer _uriComposer;
    public GetBasketEndpoint(IUriComposer uriComposer)
    {
        _uriComposer = uriComposer;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/basket", async (string buyerId, IRepository<Basket> basketRepo, IRepository<CatalogItem> itemRepo, IUriComposer composer) =>
        {
            var request = new GetBasketRequest { BuyerId = buyerId };
            var ep = new GetBasketEndpoint(composer);
            return await ep.HandleAsync(request, basketRepo, itemRepo);
        }).Produces<BasketResponse>()
        .WithTags("BasketEndpoints")
        .RequireAuthorization();
    }

    // Added overload matching interface
    public Task<IResult> HandleAsync(GetBasketRequest request, IRepository<Basket> basketRepository)
        => HandleAsync(request, basketRepository, null!);

    public async Task<IResult> HandleAsync(GetBasketRequest request, IRepository<Basket> basketRepository, IRepository<CatalogItem> itemRepository)
    {
        var spec = new BasketWithItemsSpecification(request.BuyerId);
        var basket = await basketRepository.FirstOrDefaultAsync(spec);
        if (basket == null)
        {
            basket = new Basket(request.BuyerId);
            await basketRepository.AddAsync(basket);
        }

        BasketDto dto;
        if (itemRepository != null)
        {
            dto = await MapAsync(basket, itemRepository);
        }
        else
        {
            dto = new BasketDto { Id = basket.Id, BuyerId = basket.BuyerId };
        }
        var response = new BasketResponse(request.CorrelationId()) { Basket = dto };
        return Results.Ok(response);
    }

    private async Task<BasketDto> MapAsync(Basket basket, IRepository<CatalogItem> itemRepository)
    {
        var dto = new BasketDto { Id = basket.Id, BuyerId = basket.BuyerId };
        if (!basket.Items.Any()) return dto;
        var itemIds = basket.Items.Select(i => i.CatalogItemId).ToArray();
        var catalogItems = await itemRepository.ListAsync(new CatalogItemsSpecification(itemIds));
        dto.Items.AddRange(basket.Items.Select(bItem =>
        {
            var cItem = catalogItems.First(ci => ci.Id == bItem.CatalogItemId);
            return new BasketItemDto
            {
                Id = bItem.Id,
                CatalogItemId = bItem.CatalogItemId,
                Quantity = bItem.Quantity,
                UnitPrice = bItem.UnitPrice,
                ProductName = cItem.Name,
                PictureUrl = _uriComposer.ComposePicUri(cItem.PictureUri)
            };
        }));
        return dto;
    }
}
