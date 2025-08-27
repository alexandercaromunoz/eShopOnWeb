using System.Net.Http.Json;
using BlazorShared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Infrastructure.ApiClients;

public class OrderApiClient : IOrderApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderApiClient> _logger;

    public OrderApiClient(HttpClient httpClient, IOptions<BaseUrlConfiguration> urlOptions, ILogger<OrderApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var urls = urlOptions.Value;
        _httpClient.BaseAddress = new Uri(urls.ApiBase);
    }

    public async Task<int> CreateOrderAsync(int basketId, AddressViewModel address)
    {
        var request = new CreateOrderRequest
        {
            BasketId = basketId,
            Street = address.Street,
            City = address.City,
            State = address.State,
            Country = address.Country,
            ZipCode = address.ZipCode
        };
        var result = await _httpClient.PostAsJsonAsync("orders", request);
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<CreateOrderResponse>() ?? new CreateOrderResponse();
        return response.OrderId;
    }

    public async Task<IEnumerable<OrderViewModel>> ListOrdersAsync(string buyerId)
    {
        var response = await _httpClient.GetFromJsonAsync<ListOrdersResponse>($"orders?buyerId={Uri.EscapeDataString(buyerId)}")
                       ?? new ListOrdersResponse();
        return response.Orders.Select(o => new OrderViewModel
        {
            OrderNumber = o.Id,
            OrderDate = o.OrderDate,
            Total = o.Total,
            ShippingAddress = null
        });
    }

    public async Task<OrderDetailViewModel?> GetOrderAsync(int orderId)
    {
        var response = await _httpClient.GetFromJsonAsync<OrderDetailResponse>($"orders/{orderId}");
        if (response == null) return null;
        var dto = response.Order;
        var vm = new OrderDetailViewModel
        {
            OrderNumber = dto.Id,
            OrderDate = dto.OrderDate,
            Total = dto.Total,
            ShippingAddress = new ShippingAddressViewModel { Street = dto.Street, City = dto.City, State = dto.State, Country = dto.Country, ZipCode = dto.ZipCode },
            OrderItems = dto.Items.Select(i => new OrderItemViewModel
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Units = i.Units,
                PictureUrl = i.PictureUrl
            }).ToList()
        };
        return vm;
    }

    private class CreateOrderRequest
    {
        public int BasketId { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }
    private class CreateOrderResponse { public int OrderId { get; set; } }
    private class OrderSummaryDto { public int Id { get; set; } public DateTimeOffset OrderDate { get; set; } public decimal Total { get; set; } }
    private class ListOrdersResponse { public List<OrderSummaryDto> Orders { get; set; } = new(); }
    private class OrderItemDto { public int ProductId { get; set; } public string ProductName { get; set; } = string.Empty; public decimal UnitPrice { get; set; } public int Units { get; set; } public string PictureUrl { get; set; } = string.Empty; }
    private class OrderDetailDto : OrderSummaryDto { public string Street { get; set; } = string.Empty; public string City { get; set; } = string.Empty; public string State { get; set; } = string.Empty; public string Country { get; set; } = string.Empty; public string ZipCode { get; set; } = string.Empty; public List<OrderItemDto> Items { get; set; } = new(); }
    private class OrderDetailResponse { public OrderDetailDto Order { get; set; } = new(); }
}
