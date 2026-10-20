
using FixFlow.Models.Entities;
using FixFlow.Web.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixFlow.Web.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(ValidationConstants.Customer.FullNameMaxLength);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(ValidationConstants.Customer.PhoneNumberMaxLength);

        builder.Property(c => c.Email)
            .HasMaxLength(ValidationConstants.Customer.EmailMaxLength);

        builder.Property(c => c.CreatedOn)
            .IsRequired()
            .HasDefaultValueSql(DatabaseConstants.UtcDateSql);

        builder.HasMany(c => c.RepairRequests)
            .WithOne(r => r.Customer)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
