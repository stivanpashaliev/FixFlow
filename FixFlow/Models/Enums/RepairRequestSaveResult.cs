namespace FixFlow.Models.Enums;

public enum RepairRequestSaveResult
{
    Success,
    NotFound,
    InvalidCustomer,
    InvalidTechnician,
    InactiveTechnician,
    TechnicianRequiredForInProgress,
    ClosedForEditing,
    ConcurrentModification
}
