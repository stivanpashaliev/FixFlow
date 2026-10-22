using System.ComponentModel.DataAnnotations;
using FixFlow.Models.Enums;
using FixFlow.Web.Common.Constants;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FixFlow.ViewModels.RepairRequests;

public class ChangeRepairStatusViewModel
{
    [BindNever]
    public int RepairRequestId { get; set; }

    [BindNever]
    public string DeviceName { get; set; } = string.Empty;

    [BindNever]
    public RepairStatus CurrentStatus { get; set; }

    // Posted with the form so the service can reject stale browser tabs.
    [Required]
    public RepairStatus? ExpectedStatus { get; set; }

    [Required(ErrorMessage = "Please select a new status.")]
    [Display(Name = "New Status")]
    public RepairStatus? NewStatus { get; set; }

    [StringLength(ValidationConstants.RepairStatusHistory.NoteMaxLength,
        ErrorMessage = "Note cannot exceed 500 characters.")]
    public string? Note { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> AvailableStatuses { get; set; } =
        new List<SelectListItem>();
}
