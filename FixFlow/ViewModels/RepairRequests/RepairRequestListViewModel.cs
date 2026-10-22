
using FixFlow.Models.Enums;
using FixFlow.Common.Rules;

namespace FixFlow.ViewModels.RepairRequests;

public class RepairRequestListViewModel
{
    public int Id { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string? TechnicianName { get; set; }

    public RepairStatus Status { get; set; }

    public decimal TotalCost { get; set; }

    public DateTime CreatedOn { get; set; }

    public bool CanEdit => RepairStatusRules.CanEdit(Status);

    public bool CanDelete => RepairStatusRules.CanDelete(Status);
}
