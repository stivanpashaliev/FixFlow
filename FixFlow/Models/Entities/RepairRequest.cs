using FixFlow.Models.Enums;

namespace FixFlow.Models.Entities;

public class RepairRequest
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int? TechnicianId { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public string ProblemDescription { get; set; } = string.Empty;

    public string? Diagnosis { get; set; }

    public RepairStatus Status { get; set; }

    public decimal LaborCost { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public Customer Customer { get; set; } = null!;

    public Technician? Technician { get; set; }

    public ICollection<RepairPart> RepairParts { get; set; } = new List<RepairPart>();

    public ICollection<RepairStatusHistory> StatusHistory { get; set; } =
        new List<RepairStatusHistory>();
}