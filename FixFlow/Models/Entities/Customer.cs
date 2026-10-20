namespace FixFlow.Models.Entities;

public class Customer
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime CreatedOn { get; set; }

    public ICollection<RepairRequest> RepairRequests { get; set; } = new List<RepairRequest>();
}