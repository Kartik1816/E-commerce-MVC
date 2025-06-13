using System.ComponentModel.DataAnnotations;

namespace Demo.Web.Models;

public class ContactUsViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces.")]
    [MaxLength(20, ErrorMessage = "Name cannot exceed 20 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Subject is required.")]
    public string Subject { get; set; } = null!;
    [Required(ErrorMessage = "Message is required.")]
    public string Message { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required.")]
    [MaxLength(10, ErrorMessage = "Phone number cannot exceed 10 digits.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be a 10-digit number.")]
    public string PhoneNumber { get; set; } = null!;
}
