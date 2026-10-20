
using FixFlow.Models.Enums;

namespace FixFlow.ViewModels.RepairRequests;

public class RepairRequestDetailsViewModel
{
    public int Id { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public string ProblemDescription { get; set; } = string.Empty;

    public string? Diagnosis { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string? TechnicianName { get; set; }

    public RepairStatus Status { get; set; }

    public decimal LaborCost { get; set; }

    public decimal PartsCost { get; set; }

    public decimal TotalCost => LaborCost + PartsCost;

    public DateTime CreatedOn { get; set; }

    public DateTime? CompletedOn { get; set; }
}
