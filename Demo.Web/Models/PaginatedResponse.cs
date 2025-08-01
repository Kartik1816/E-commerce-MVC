namespace Demo.Web.Models;

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int TotalDataOfPage { get; set; }
}
