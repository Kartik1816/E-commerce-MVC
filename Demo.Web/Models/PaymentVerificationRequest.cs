namespace Demo.Web.Models;

public class PaymentVerificationRequest
{
    public string PaymentId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string Signature { get; set; } = null!;

}
