
using System.ComponentModel.DataAnnotations;
using FixFlow.Models.Enums;
using FixFlow.Web.Common.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FixFlow.ViewModels.RepairRequests;

public class RepairRequestFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a customer.")]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Display(Name = "Technician")]
    public int? TechnicianId { get; set; }

    [Required(ErrorMessage = "Device name is required.")]
    [StringLength(ValidationConstants.RepairRequest.DeviceNameMaxLength)]
    [Display(Name = "Device Name")]
    public string DeviceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Problem description is required.")]
    [StringLength(ValidationConstants.RepairRequest.ProblemDescriptionMaxLength)]
    [Display(Name = "Problem Description")]
    public string ProblemDescription { get; set; } = string.Empty;

    [StringLength(ValidationConstants.RepairRequest.DiagnosisMaxLength)]
    public string? Diagnosis { get; set; }

    [Display(Name = "Status")]
    public RepairStatus Status { get; set; } = RepairStatus.New;

    [Range(typeof(decimal), "0", "9999999999999999.99",
        ErrorMessage = "Labor cost cannot be negative.")]
    [Display(Name = "Labor Cost")]
    public decimal LaborCost { get; set; }

    public IEnumerable<SelectListItem> Customers { get; set; }
        = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Technicians { get; set; }
        = new List<SelectListItem>();
}
