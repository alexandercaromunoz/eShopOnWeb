namespace Microsoft.eShopWeb.Web.ViewModels;

public class PaginationInfoViewModel
{
    public int ActualPage { get; set; }
    public int ItemsPerPage { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public string Previous => ActualPage <= 0 ? "esh-pager-item--disabled" : string.Empty;
    public string Next => ActualPage >= TotalPages - 1 ? "esh-pager-item--disabled" : string.Empty;
}
