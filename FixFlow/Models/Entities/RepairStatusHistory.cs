using FixFlow.Models.Enums;

namespace FixFlow.Models.Entities;

public class RepairStatusHistory
{
    public int Id { get; set; }

    public int RepairRequestId { get; set; }

    public RepairStatus OldStatus { get; set; }

    public RepairStatus NewStatus { get; set; }

    public DateTime ChangedOn { get; set; }

    public string? Note { get; set; }

    public RepairRequest RepairRequest { get; set; } = null!;
}