namespace FixFlow.Models.Enums;

public enum RepairRequestDeleteResult
{
    Success,
    NotFound,
    HasRepairParts,
    NotAllowedInCurrentStatus
}
