using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.OrderEndpoints;

[Authorize]
public class GetOrderEndpoint : IEndpoint<IResult, GetOrderRequest, IRepository<Order>>
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/orders/{orderId:int}", async (int orderId, IRepository<Order> orderRepo) =>
        {
            return await HandleAsync(new GetOrderRequest { OrderId = orderId }, orderRepo);
        }).Produces<OrderDetailResponse>()
          .WithTags("OrderEndpoints")
          .RequireAuthorization();
    }

    public async Task<IResult> HandleAsync(GetOrderRequest request, IRepository<Order> orderRepository)
    {
        var spec = new OrderWithItemsByIdSpec(request.OrderId);
        var order = await orderRepository.FirstOrDefaultAsync(spec);
        if (order == null)
        {
            return Results.NotFound();
        }

        var dto = new OrderDetailDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Total = order.Total(),
            Street = order.ShipToAddress.Street,
            City = order.ShipToAddress.City,
            State = order.ShipToAddress.State,
            Country = order.ShipToAddress.Country,
            ZipCode = order.ShipToAddress.ZipCode,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ItemOrdered.CatalogItemId,
                ProductName = oi.ItemOrdered.ProductName,
                UnitPrice = oi.UnitPrice,
                Units = oi.Units,
                PictureUrl = oi.ItemOrdered.PictureUri
            }).ToList()
        };

        return Results.Ok(new OrderDetailResponse(request.CorrelationId()) { Order = dto });
    }
}
