namespace Demo.Web.Models;

public class PaginationRequestModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
