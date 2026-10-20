
using FixFlow.Models.Entities;
using FixFlow.Web.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixFlow.Web.Data.Configurations;

public class SparePartConfiguration : IEntityTypeConfiguration<SparePart>
{
    public void Configure(EntityTypeBuilder<SparePart> builder)
    {
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(ValidationConstants.SparePart.NameMaxLength);

        builder.Property(sp => sp.SKU)
            .IsRequired()
            .HasMaxLength(ValidationConstants.SparePart.SkuMaxLength);

        builder.HasIndex(sp => sp.SKU)
            .IsUnique();

        builder.Property(sp => sp.UnitPrice)
            .HasPrecision(
                DatabaseConstants.MoneyPrecision,
                DatabaseConstants.MoneyScale);

        builder.Property(sp => sp.QuantityInStock)
            .HasDefaultValue(0);
    }
}
