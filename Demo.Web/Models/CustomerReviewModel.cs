namespace Demo.Web.Models;

public class CustomerReviewModel
{
    public int Rating { get; set; }

    public int ProductId { get; set; }

    public string? Comment { get; set; }
    
    public int UserId { get; set; }
}
