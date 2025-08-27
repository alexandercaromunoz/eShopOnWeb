using MediatR;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.eShopWeb.Web.Interfaces;

namespace Microsoft.eShopWeb.Web.Features.MyOrders;

public class GetMyOrdersHandler : IRequestHandler<GetMyOrders, IEnumerable<OrderViewModel>>
{
    private readonly IOrderApiClient _orderApiClient;

    public GetMyOrdersHandler(IOrderApiClient orderApiClient)
    {
        _orderApiClient = orderApiClient;
    }

    public async Task<IEnumerable<OrderViewModel>> Handle(GetMyOrders request,
        CancellationToken cancellationToken)
    {
        return await _orderApiClient.ListOrdersAsync(request.UserName);
    }
}
