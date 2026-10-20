namespace FixFlow.Web.Common.Constants;

public static class ErrorMessages
{
    public const string RequiredField =
        "This field is required.";

    public const string InsufficientStock =
        "Not enough spare parts are available in stock.";

    public const string InvalidStatusTransition =
        "This repair status transition is not allowed.";

    public const string TechnicianRequired =
        "A technician must be assigned before completing a repair.";

    public const string DiagnosisRequired =
        "A diagnosis is required before completing a repair.";
}