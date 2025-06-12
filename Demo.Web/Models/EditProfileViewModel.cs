using System.ComponentModel.DataAnnotations;

namespace Demo.Web.Models;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(128, ErrorMessage = "Email cannot exceed 128 characters.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    public string Email { get; set; } = null!;

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [MaxLength(50, ErrorMessage = "Password cannot exceed 50 characters.")]
    [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "Please Enter correct Password")]
    public string? Password { get; set; } 

    [MinLength(8, ErrorMessage = "Confirm Password must be at least 8 characters.")]
    [MaxLength(50, ErrorMessage = "Confirm Password cannot exceed 50 characters.")]
    [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "Please Enter correct Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; } 

    [Required(ErrorMessage = "First Name is required.")]
    [MaxLength(20, ErrorMessage = "First Name cannot exceed 20 characters.")]
    [RegularExpression(@"^[a-zA-Z]{2,}$", ErrorMessage = "First Name must be at least 2 characters long and can only contain letters and spaces.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last Name is required.")]
    [MaxLength(20, ErrorMessage = "Last Name cannot exceed 20 characters.")]
    [RegularExpression(@"^[a-zA-Z]{2,}$", ErrorMessage = "Last Name must be at least 2 characters long and can only contain letters and spaces.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Phone Number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone Number must be 10 digits.")]
    [MaxLength(10, ErrorMessage = "Phone Number cannot exceed 10 digits.")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Address is required.")]
    [RegularExpression(@"^[a-zA-Z0-9\s,.'-]{3,}$", ErrorMessage = "Address must be at least 3 characters long and can include letters, numbers, spaces, commas, periods, and hyphens.")]
    [MinLength(3, ErrorMessage = "Address must be at least 3 characters long.")]
    public string Address { get; set; } = null!;

    public IFormFile? Image { get; set; } 
    public int RoleId { get; set; }
    public string? ImageUrl { get; set; }
    public int Id { get; set; }
}
