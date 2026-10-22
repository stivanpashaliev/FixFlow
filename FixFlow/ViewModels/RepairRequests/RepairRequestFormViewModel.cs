using System.ComponentModel.DataAnnotations;
using FixFlow.Models.Enums;
using FixFlow.Web.Common.Constants;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FixFlow.ViewModels.RepairRequests;

public class RepairRequestFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a customer.")]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid technician.")]
    [Display(Name = "Technician")]
    public int? TechnicianId { get; set; }

    [Required(ErrorMessage = "Device name is required.")]
    [StringLength(ValidationConstants.RepairRequest.DeviceNameMaxLength,
        MinimumLength = ValidationConstants.RepairRequest.DeviceNameMinLength)]
    [Display(Name = "Device Name")]
    public string DeviceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Problem description is required.")]
    [StringLength(ValidationConstants.RepairRequest.ProblemDescriptionMaxLength,
        MinimumLength = ValidationConstants.RepairRequest.ProblemDescriptionMinLength)]
    [Display(Name = "Problem Description")]
    public string ProblemDescription { get; set; } = string.Empty;

    [StringLength(ValidationConstants.RepairRequest.DiagnosisMaxLength)]
    public string? Diagnosis { get; set; }

    // Read-only here: future status transitions must also create history records.
    [BindNever]
    [Display(Name = "Status")]
    public RepairStatus Status { get; set; } = RepairStatus.New;

    [Range(typeof(decimal), "0", "9999999999999999.99",
        ErrorMessage = "Labor cost must be zero or greater and fit the allowed range.")]
    [Display(Name = "Labor Cost")]
    public decimal LaborCost { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Customers { get; set; } = new List<SelectListItem>();

    [BindNever]
    public IEnumerable<SelectListItem> Technicians { get; set; } = new List<SelectListItem>();
}
