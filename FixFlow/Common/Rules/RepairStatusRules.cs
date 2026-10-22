using FixFlow.Models.Enums;

namespace FixFlow.Common.Rules;

// One place for the allowed repair status transitions.
public static class RepairStatusRules
{
    public static IReadOnlyList<RepairStatus> GetAllowedNextStatuses(RepairStatus currentStatus)
    {
        return currentStatus switch
        {
            RepairStatus.New => new[] { RepairStatus.Diagnosing, RepairStatus.Cancelled },
            RepairStatus.Diagnosing => new[] { RepairStatus.InProgress, RepairStatus.Cancelled },
            RepairStatus.InProgress => new[] { RepairStatus.Completed, RepairStatus.Cancelled },
            RepairStatus.Completed => new[] { RepairStatus.Delivered },
            _ => Array.Empty<RepairStatus>()
        };
    }

    public static bool IsAllowed(RepairStatus currentStatus, RepairStatus newStatus)
    {
        return GetAllowedNextStatuses(currentStatus).Contains(newStatus);
    }

    public static bool CanEdit(RepairStatus status)
    {
        return status is RepairStatus.New or RepairStatus.Diagnosing or RepairStatus.InProgress;
    }

    public static bool CanDelete(RepairStatus status)
    {
        // Once work starts, the repair becomes part of the shop's history.
        return status == RepairStatus.New;
    }
}
