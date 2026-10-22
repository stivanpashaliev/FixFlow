
using System.ComponentModel.DataAnnotations;
using FixFlow.Web.Common.Constants;

namespace FixFlow.ViewModels.Customers;

public class CustomerFormViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(
        ValidationConstants.Customer.FullNameMaxLength,
        MinimumLength = ValidationConstants.Customer.FullNameMinLength,
        ErrorMessage = "Full name must be between 2 and 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(ValidationConstants.Customer.PhoneNumberMaxLength)]
    [RegularExpression(
        @"^\+?[0-9]{7,15}$",
        ErrorMessage = "Enter a valid phone number (7-15 digits, optional leading +).")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(ValidationConstants.Customer.EmailMaxLength)]
    public string? Email { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FullName is null)
        {
            yield break;
        }

        string normalizedName = FullName.Trim();

        if (normalizedName.Length < ValidationConstants.Customer.FullNameMinLength ||
            normalizedName.Length > ValidationConstants.Customer.FullNameMaxLength)
        {
            yield return new ValidationResult(
                "Full name must be between 2 and 100 characters after trimming.",
                new[] { nameof(FullName) });
        }
    }
}