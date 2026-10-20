
using FixFlow.Models.Entities;
using FixFlow.Web.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixFlow.Web.Data.Configurations;

public class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FullName)
            .IsRequired()
            .HasMaxLength(ValidationConstants.Technician.FullNameMaxLength);

        builder.Property(t => t.Specialization)
            .HasMaxLength(ValidationConstants.Technician.SpecializationMaxLength);

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(t => t.RepairRequests)
            .WithOne(r => r.Technician)
            .HasForeignKey(r => r.TechnicianId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
