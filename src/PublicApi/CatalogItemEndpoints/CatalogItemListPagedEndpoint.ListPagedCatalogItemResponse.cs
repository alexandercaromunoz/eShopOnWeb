using System;
using System.Collections.Generic;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

public class ListPagedCatalogItemResponse : BaseResponse
{
    public ListPagedCatalogItemResponse(Guid correlationId) : base(correlationId)
    {
    }

    public ListPagedCatalogItemResponse()
    {
    }

    public List<CatalogItemDto> CatalogItems { get; set; } = new List<CatalogItemDto>();
    public int PageCount { get; set; }
    // Total number of items matching the filter (for UI pagination)
    public int TotalItems { get; set; }
}
