
namespace FixFlow.ViewModels.Customers;

public class CustomerDetailsViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime CreatedOn { get; set; }

    public int RepairRequestsCount { get; set; }
}
