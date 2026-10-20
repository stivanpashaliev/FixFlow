namespace FixFlow.Models.Entities;

public class SparePart
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int QuantityInStock { get; set; }

    public ICollection<RepairPart> RepairParts { get; set; } = new List<RepairPart>();
}