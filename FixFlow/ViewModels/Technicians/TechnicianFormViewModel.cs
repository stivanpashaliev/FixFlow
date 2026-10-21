using System.ComponentModel.DataAnnotations;
using FixFlow.Web.Common.Constants;

namespace FixFlow.ViewModels.Technicians;

public class TechnicianFormViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(
        ValidationConstants.Technician.FullNameMaxLength,
        MinimumLength = ValidationConstants.Technician.FullNameMinLength,
        ErrorMessage = "Full name must be between 2 and 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(
        ValidationConstants.Technician.SpecializationMaxLength,
        ErrorMessage = "Specialization cannot exceed 100 characters.")]
    public string? Specialization { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}