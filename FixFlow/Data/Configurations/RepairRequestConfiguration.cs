
using FixFlow.Models.Entities;
using FixFlow.Models.Enums;
using FixFlow.Web.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixFlow.Web.Data.Configurations;

public class RepairRequestConfiguration : IEntityTypeConfiguration<RepairRequest>
{
    public void Configure(EntityTypeBuilder<RepairRequest> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.DeviceName)
            .IsRequired()
            .HasMaxLength(ValidationConstants.RepairRequest.DeviceNameMaxLength);

        builder.Property(r => r.ProblemDescription)
            .IsRequired()
            .HasMaxLength(ValidationConstants.RepairRequest.ProblemDescriptionMaxLength);

        builder.Property(r => r.Diagnosis)
            .HasMaxLength(ValidationConstants.RepairRequest.DiagnosisMaxLength);

        builder.Property(r => r.Status)
            .HasConversion<int>()
            .HasDefaultValue(RepairStatus.New);

        builder.Property(r => r.LaborCost)
            .HasPrecision(
                DatabaseConstants.MoneyPrecision,
                DatabaseConstants.MoneyScale)
            .HasDefaultValue(0m);

        builder.Property(r => r.CreatedOn)
            .HasDefaultValueSql(DatabaseConstants.UtcDateSql);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedOn);

        builder.HasMany(r => r.StatusHistory)
            .WithOne(h => h.RepairRequest)
            .HasForeignKey(h => h.RepairRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
