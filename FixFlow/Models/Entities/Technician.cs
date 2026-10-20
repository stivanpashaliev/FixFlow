namespace FixFlow.Models.Entities;

public class Technician
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    public bool IsActive { get; set; }

    public ICollection<RepairRequest> RepairRequests { get; set; } = new List<RepairRequest>();
}