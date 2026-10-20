
using FixFlow.Models.Entities;
using FixFlow.Models.Enums;
using FixFlow.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(FixFlowDbContext context)
    {
        if (!await context.Customers.AnyAsync())
        {
            context.Customers.AddRange(
                new Customer
                {
                    FullName = "John Smith",
                    PhoneNumber = "0888000001",
                    Email = "john@example.com",
                    CreatedOn = DateTime.UtcNow
                },
                new Customer
                {
                    FullName = "Emily Johnson",
                    PhoneNumber = "0888000002",
                    Email = "emily@example.com",
                    CreatedOn = DateTime.UtcNow
                },
                new Customer
                {
                    FullName = "Michael Brown",
                    PhoneNumber = "0888000003",
                    Email = "michael@example.com",
                    CreatedOn = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.Technicians.AnyAsync())
        {
            context.Technicians.AddRange(
                new Technician
                {
                    FullName = "Daniel Wilson",
                    Specialization = "Laptop Repairs",
                    IsActive = true
                },
                new Technician
                {
                    FullName = "James Anderson",
                    Specialization = "Smartphone Repairs",
                    IsActive = true
                }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.SpareParts.AnyAsync())
        {
            context.SpareParts.AddRange(
                new SparePart
                {
                    Name = "SSD 512GB",
                    SKU = "SSD-512",
                    UnitPrice = 89.90m,
                    QuantityInStock = 10
                },
                new SparePart
                {
                    Name = "RAM 16GB",
                    SKU = "RAM-16",
                    UnitPrice = 69.90m,
                    QuantityInStock = 8
                },
                new SparePart
                {
                    Name = "Laptop Battery",
                    SKU = "BAT-LAP",
                    UnitPrice = 119.00m,
                    QuantityInStock = 6
                },
                new SparePart
                {
                    Name = "Phone Display",
                    SKU = "DIS-PHN",
                    UnitPrice = 149.00m,
                    QuantityInStock = 5
                },
                new SparePart
                {
                    Name = "Cooling Fan",
                    SKU = "FAN-001",
                    UnitPrice = 29.90m,
                    QuantityInStock = 12
                }
            );

            await context.SaveChangesAsync();
        }

        if (!await context.RepairRequests.AnyAsync())
        {
            var customers = await context.Customers
                .OrderBy(c => c.Id)
                .Take(3)
                .ToListAsync();

            var technicians = await context.Technicians
                .OrderBy(t => t.Id)
                .Take(2)
                .ToListAsync();

            var parts = await context.SpareParts
                .Where(p =>
                    p.SKU == "SSD-512" ||
                    p.SKU == "RAM-16" ||
                    p.SKU == "BAT-LAP" ||
                    p.SKU == "FAN-001")
                .ToDictionaryAsync(p => p.SKU);

            if (customers.Count < 3 ||
                technicians.Count < 2 ||
                parts.Count < 4)
            {
                throw new InvalidOperationException(
                    "Not enough customers, technicians or spare parts to seed repairs.");
            }

            var now = DateTime.UtcNow;

            var repair1 = new RepairRequest
            {
                CustomerId = customers[0].Id,
                DeviceName = "Lenovo ThinkPad T480",
                ProblemDescription = "The laptop does not turn on.",
                Status = RepairStatus.New,
                LaborCost = 0m,
                CreatedOn = now.AddDays(-1)
            };

            var repair2 = new RepairRequest
            {
                CustomerId = customers[1].Id,
                TechnicianId = technicians[1].Id,
                DeviceName = "Samsung Galaxy S22",
                ProblemDescription = "The phone shuts down unexpectedly.",
                Status = RepairStatus.Diagnosing,
                LaborCost = 0m,
                CreatedOn = now.AddDays(-2)
            };

            var repair3 = new RepairRequest
            {
                CustomerId = customers[2].Id,
                TechnicianId = technicians[0].Id,
                DeviceName = "Dell Inspiron 15",
                ProblemDescription = "The laptop is running very slowly.",
                Diagnosis = "The hard drive is faulty and needs to be replaced with an SSD.",
                Status = RepairStatus.InProgress,
                LaborCost = 35m,
                CreatedOn = now.AddDays(-4)
            };

            var repair4 = new RepairRequest
            {
                CustomerId = customers[0].Id,
                TechnicianId = technicians[0].Id,
                DeviceName = "HP Pavilion 15",
                ProblemDescription = "The laptop overheats and performs poorly.",
                Diagnosis = "The cooling fan is faulty and the RAM needs to be upgraded.",
                Status = RepairStatus.Completed,
                LaborCost = 50m,
                CreatedOn = now.AddDays(-8),
                CompletedOn = now.AddDays(-2)
            };

            var repair5 = new RepairRequest
            {
                CustomerId = customers[1].Id,
                TechnicianId = technicians[1].Id,
                DeviceName = "iPhone 13",
                ProblemDescription = "The battery drains very quickly.",
                Diagnosis = "The battery is worn out and needs replacement.",
                Status = RepairStatus.Delivered,
                LaborCost = 40m,
                CreatedOn = now.AddDays(-12),
                CompletedOn = now.AddDays(-5)
            };

            context.RepairRequests.AddRange(
                repair1,
                repair2,
                repair3,
                repair4,
                repair5
            );

            await context.SaveChangesAsync();

            var ssd = parts["SSD-512"];
            var ram = parts["RAM-16"];
            var battery = parts["BAT-LAP"];
            var fan = parts["FAN-001"];

            if (ssd.QuantityInStock < 1 ||
                ram.QuantityInStock < 1 ||
                battery.QuantityInStock < 1 ||
                fan.QuantityInStock < 1)
            {
                throw new InvalidOperationException(
                    "Not enough spare parts in stock to seed repairs.");
            }

            context.RepairParts.AddRange(
                new RepairPart
                {
                    RepairRequestId = repair3.Id,
                    SparePartId = ssd.Id,
                    Quantity = 1,
                    UnitPriceAtUse = ssd.UnitPrice
                },
                new RepairPart
                {
                    RepairRequestId = repair4.Id,
                    SparePartId = ram.Id,
                    Quantity = 1,
                    UnitPriceAtUse = ram.UnitPrice
                },
                new RepairPart
                {
                    RepairRequestId = repair4.Id,
                    SparePartId = fan.Id,
                    Quantity = 1,
                    UnitPriceAtUse = fan.UnitPrice
                },
                new RepairPart
                {
                    RepairRequestId = repair5.Id,
                    SparePartId = battery.Id,
                    Quantity = 1,
                    UnitPriceAtUse = battery.UnitPrice
                }
            );

            ssd.QuantityInStock--;
            ram.QuantityInStock--;
            fan.QuantityInStock--;
            battery.QuantityInStock--;

            context.RepairStatusHistories.AddRange(
                new RepairStatusHistory
                {
                    RepairRequestId = repair1.Id,
                    OldStatus = RepairStatus.New,
                    NewStatus = RepairStatus.New,
                    ChangedOn = repair1.CreatedOn,
                    Note = "The repair request was registered."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair2.Id,
                    OldStatus = RepairStatus.New,
                    NewStatus = RepairStatus.Diagnosing,
                    ChangedOn = repair2.CreatedOn.AddHours(2),
                    Note = "Device diagnosis has started."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair3.Id,
                    OldStatus = RepairStatus.New,
                    NewStatus = RepairStatus.Diagnosing,
                    ChangedOn = repair3.CreatedOn.AddHours(1),
                    Note = "The device was accepted for diagnosis."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair3.Id,
                    OldStatus = RepairStatus.Diagnosing,
                    NewStatus = RepairStatus.InProgress,
                    ChangedOn = repair3.CreatedOn.AddDays(1),
                    Note = "SSD replacement has started."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair4.Id,
                    OldStatus = RepairStatus.New,
                    NewStatus = RepairStatus.Diagnosing,
                    ChangedOn = repair4.CreatedOn.AddHours(2),
                    Note = "Device diagnosis has started."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair4.Id,
                    OldStatus = RepairStatus.Diagnosing,
                    NewStatus = RepairStatus.InProgress,
                    ChangedOn = repair4.CreatedOn.AddDays(1),
                    Note = "Repair work has started."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair4.Id,
                    OldStatus = RepairStatus.InProgress,
                    NewStatus = RepairStatus.Completed,
                    ChangedOn = repair4.CompletedOn!.Value,
                    Note = "The repair was completed successfully."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair5.Id,
                    OldStatus = RepairStatus.New,
                    NewStatus = RepairStatus.Diagnosing,
                    ChangedOn = repair5.CreatedOn.AddHours(1),
                    Note = "Battery health was checked."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair5.Id,
                    OldStatus = RepairStatus.Diagnosing,
                    NewStatus = RepairStatus.InProgress,
                    ChangedOn = repair5.CreatedOn.AddDays(1),
                    Note = "Battery replacement has started."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair5.Id,
                    OldStatus = RepairStatus.InProgress,
                    NewStatus = RepairStatus.Completed,
                    ChangedOn = repair5.CompletedOn!.Value,
                    Note = "The battery was replaced successfully."
                },
                new RepairStatusHistory
                {
                    RepairRequestId = repair5.Id,
                    OldStatus = RepairStatus.Completed,
                    NewStatus = RepairStatus.Delivered,
                    ChangedOn = repair5.CompletedOn!.Value.AddDays(1),
                    Note = "The repaired device was returned to the customer."
                }
            );

            await context.SaveChangesAsync();
        }
    }
}