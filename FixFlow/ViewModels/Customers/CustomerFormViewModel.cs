using System.ComponentModel.DataAnnotations;
using FixFlow.Web.Common.Constants;

namespace FixFlow.ViewModels.Customers;

public class CustomerFormViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(ValidationConstants.Customer.FullNameMaxLength)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(ValidationConstants.Customer.PhoneNumberMaxLength)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(ValidationConstants.Customer.EmailMaxLength)]
    public string? Email { get; set; }
}