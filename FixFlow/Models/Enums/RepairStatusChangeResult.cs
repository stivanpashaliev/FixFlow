namespace FixFlow.Models.Enums;

public enum RepairStatusChangeResult
{
    Success,
    NotFound,
    InvalidTransition,
    MissingTechnician,
    MissingDiagnosis,
    CancellationNoteRequired,
    CannotCancelWithParts,
    InvalidNote,
    ConcurrentModification
}
