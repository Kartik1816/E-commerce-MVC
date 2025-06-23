namespace Demo.Web.Models;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
}
