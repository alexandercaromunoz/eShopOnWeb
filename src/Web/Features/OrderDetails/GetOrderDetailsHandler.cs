using MediatR;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.eShopWeb.Web.Interfaces;

namespace Microsoft.eShopWeb.Web.Features.OrderDetails;

public class GetOrderDetailsHandler : IRequestHandler<GetOrderDetails, OrderDetailViewModel?>
{
    private readonly IOrderApiClient _orderApiClient;

    public GetOrderDetailsHandler(IOrderApiClient orderApiClient)
    {
        _orderApiClient = orderApiClient;
    }

    public async Task<OrderDetailViewModel?> Handle(GetOrderDetails request,
        CancellationToken cancellationToken)
    {
        var detail = await _orderApiClient.GetOrderAsync(request.OrderId);
        // Optionally ensure buyer matches (request.UserName) client-side if API enforces auth
        return detail;
    }
}
