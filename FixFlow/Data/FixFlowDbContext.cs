using FixFlow.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Web.Data;

public class FixFlowDbContext : DbContext
{
    public FixFlowDbContext(
        DbContextOptions<FixFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Technician> Technicians => Set<Technician>();

    public DbSet<RepairRequest> RepairRequests
        => Set<RepairRequest>();

    public DbSet<SparePart> SpareParts => Set<SparePart>();

    public DbSet<RepairPart> RepairParts => Set<RepairPart>();

    public DbSet<RepairStatusHistory> RepairStatusHistories
        => Set<RepairStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FixFlowDbContext).Assembly);
    }
}