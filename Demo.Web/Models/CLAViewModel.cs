namespace Demo.Web.Models;

public class CLAViewModel
{
    public ProductViewModel? productViewModel { get; set; }
    public List<CategoryViewModel>? Categories { get; set; }
    public List<ProductViewModel>? Products { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int TotalDataOfPage { get; set; }
}
