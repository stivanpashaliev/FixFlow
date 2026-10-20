
namespace FixFlow.ViewModels.Technicians;

public class TechnicianDetailsViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    public bool IsActive { get; set; }

    public int RepairRequestsCount { get; set; }
}
