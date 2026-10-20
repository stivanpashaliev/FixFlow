namespace FixFlow.Models.Entities;

public class RepairPart
{
    public int Id { get; set; }

    public int RepairRequestId { get; set; }

    public int SparePartId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPriceAtUse { get; set; }

    public RepairRequest RepairRequest { get; set; } = null!;

    public SparePart SparePart { get; set; } = null!;
}