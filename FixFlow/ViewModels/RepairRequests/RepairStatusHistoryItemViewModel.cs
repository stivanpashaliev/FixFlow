using FixFlow.Models.Enums;

namespace FixFlow.ViewModels.RepairRequests;

public class RepairStatusHistoryItemViewModel
{
    public RepairStatus OldStatus { get; set; }

    public RepairStatus NewStatus { get; set; }

    public DateTime ChangedOn { get; set; }

    public string? Note { get; set; }
}
