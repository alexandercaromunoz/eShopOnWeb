using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.OrderEndpoints;

[Authorize]
public class CreateOrderEndpoint : IEndpoint<IResult, CreateOrderRequest, IRepository<Order>>
{
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;
    private readonly IUriComposer _uriComposer;

    public CreateOrderEndpoint(IRepository<Basket> basketRepository,
                               IRepository<CatalogItem> itemRepository,
                               IUriComposer uriComposer)
    {
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
        _uriComposer = uriComposer;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("api/orders", async (CreateOrderRequest request,
                                         IRepository<Order> orderRepo,
                                         IRepository<Basket> basketRepo,
                                         IRepository<CatalogItem> itemRepo,
                                         IUriComposer uriComposer) =>
        {
            var ep = new CreateOrderEndpoint(basketRepo, itemRepo, uriComposer);
            return await ep.HandleAsync(request, orderRepo);
        }).Produces<CreateOrderResponse>()
          .WithTags("OrderEndpoints")
          .RequireAuthorization();
    }

    public async Task<IResult> HandleAsync(CreateOrderRequest request, IRepository<Order> orderRepository)
    {
        var basketSpec = new BasketWithItemsSpecification(request.BasketId);
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);
        if (basket == null || !basket.Items.Any())
        {
            return Results.BadRequest("Basket not found or empty.");
        }

        var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
        var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
            return new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
        }).ToList();

        var address = new Address(request.Street, request.City, request.State, request.Country, request.ZipCode);
        var order = new Order(basket.BuyerId, address, items);
        await orderRepository.AddAsync(order);

        return Results.Ok(new CreateOrderResponse(request.CorrelationId()) { OrderId = order.Id });
    }
}
