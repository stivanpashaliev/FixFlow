using FixFlow.Models.Enums;
using FixFlow.Common.Rules;

namespace FixFlow.ViewModels.RepairRequests;

public class RepairRequestDetailsViewModel
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int? TechnicianId { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public string ProblemDescription { get; set; } = string.Empty;

    public string? Diagnosis { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string? TechnicianName { get; set; }

    public RepairStatus Status { get; set; }

    public decimal LaborCost { get; set; }

    public decimal PartsCost { get; set; }

    public decimal TotalCost => LaborCost + PartsCost;

    public int RepairPartsCount { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public bool CanChangeStatus => RepairStatusRules.GetAllowedNextStatuses(Status).Count > 0;

    public bool CanEdit => RepairStatusRules.CanEdit(Status);

    public bool CanDelete => RepairStatusRules.CanDelete(Status);

    public IReadOnlyList<RepairStatusHistoryItemViewModel> StatusHistory { get; set; } =
        new List<RepairStatusHistoryItemViewModel>();
}
