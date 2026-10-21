using System.ComponentModel.DataAnnotations;
using FixFlow.Web.Common.Constants;

namespace FixFlow.ViewModels.SpareParts;

public class SparePartFormViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Part name is required.")]
    [StringLength(ValidationConstants.SparePart.NameMaxLength)]
    [Display(Name = "Part Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(ValidationConstants.SparePart.SkuMaxLength)]
    [Display(Name = "SKU")]
    public string SKU { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ErrorMessage = "Unit price must be greater than zero and within the allowed range.")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "Quantity in stock cannot be negative.")]
    [Display(Name = "Quantity in Stock")]
    public int QuantityInStock { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (decimal.Round(UnitPrice, 2) != UnitPrice)
        {
            yield return new ValidationResult(
                "Unit price cannot have more than two decimal places.",
                new[] { nameof(UnitPrice) });
        }
    }
}
