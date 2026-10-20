
using FixFlow.Models.Entities;
using FixFlow.Web.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixFlow.Web.Data.Configurations;

public class RepairPartConfiguration : IEntityTypeConfiguration<RepairPart>
{
    public void Configure(EntityTypeBuilder<RepairPart> builder)
    {
        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.UnitPriceAtUse)
            .HasPrecision(
                DatabaseConstants.MoneyPrecision,
                DatabaseConstants.MoneyScale);

        builder.HasIndex(rp => new
        {
            rp.RepairRequestId,
            rp.SparePartId
        }).IsUnique();

        builder.HasOne(rp => rp.RepairRequest)
            .WithMany(r => r.RepairParts)
            .HasForeignKey(rp => rp.RepairRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.SparePart)
            .WithMany(sp => sp.RepairParts)
            .HasForeignKey(rp => rp.SparePartId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
