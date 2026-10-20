
namespace FixFlow.ViewModels.SpareParts;

public class SparePartListViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int QuantityInStock { get; set; }
}
