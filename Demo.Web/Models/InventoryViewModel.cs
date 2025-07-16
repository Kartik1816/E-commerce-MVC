namespace Demo.Web.Models;

public class InventoryViewModel
{
    public List<InventoryItemViewModel> InventoryItems { get; set; } = new List<InventoryItemViewModel>();
    public int TotalStock { get; set; }
    public int TotalSoldQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalRemainingStock { get; set; }

    public List<string> ChartLabels { get; set; } = new();  // e.g., Jan, Feb, Mar
    public List<decimal> ChartData { get; set; } = new();   // e.g., 12000, 15000

    // Optional future enhancements
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    
}
