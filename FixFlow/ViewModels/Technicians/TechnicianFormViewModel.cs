
using System.ComponentModel.DataAnnotations;
using FixFlow.Web.Common.Constants;

namespace FixFlow.ViewModels.Technicians;

public class TechnicianFormViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(ValidationConstants.Technician.FullNameMaxLength)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(ValidationConstants.Technician.SpecializationMaxLength)]
    public string? Specialization { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
