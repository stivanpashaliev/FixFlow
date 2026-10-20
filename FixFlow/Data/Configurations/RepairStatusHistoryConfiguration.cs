
using FixFlow.Models.Entities;
using FixFlow.Web.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixFlow.Web.Data.Configurations;

public class RepairStatusHistoryConfiguration : IEntityTypeConfiguration<RepairStatusHistory>
{
    public void Configure(EntityTypeBuilder<RepairStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.OldStatus)
            .HasConversion<int>();

        builder.Property(h => h.NewStatus)
            .HasConversion<int>();

        builder.Property(h => h.ChangedOn)
            .HasDefaultValueSql(DatabaseConstants.UtcDateSql);

        builder.Property(h => h.Note)
            .HasMaxLength(ValidationConstants.RepairStatusHistory.NoteMaxLength);

        builder.HasIndex(h => new
        {
            h.RepairRequestId,
            h.ChangedOn
        });

        builder.HasOne(h => h.RepairRequest)
            .WithMany(r => r.StatusHistory)
            .HasForeignKey(h => h.RepairRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
