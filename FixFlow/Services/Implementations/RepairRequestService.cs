using System.Data;
using FixFlow.Models.Entities;
using FixFlow.Models.Enums;
using FixFlow.Services.Interfaces;
using FixFlow.ViewModels.RepairRequests;
using FixFlow.Web.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Services.Implementations;

public class RepairRequestService : IRepairRequestService
{
    private readonly FixFlowDbContext _context;

    public RepairRequestService(FixFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RepairRequestListViewModel>> GetAllAsync()
    {
        return await _context.RepairRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedOn)
            .ThenByDescending(r => r.Id)
            .Select(r => new RepairRequestListViewModel
            {
                Id = r.Id,
                DeviceName = r.DeviceName,
                CustomerName = r.Customer.FullName,
                TechnicianName = r.Technician != null ? r.Technician.FullName : null,
                Status = r.Status,
                TotalCost = r.LaborCost +
                    (r.RepairParts.Sum(rp => (decimal?)(rp.Quantity * rp.UnitPriceAtUse)) ?? 0m),
                CreatedOn = r.CreatedOn
            })
            .ToListAsync();
    }

    public async Task<RepairRequestDetailsViewModel?> GetDetailsAsync(int id)
    {
        return await _context.RepairRequests
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RepairRequestDetailsViewModel
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                TechnicianId = r.TechnicianId,
                DeviceName = r.DeviceName,
                ProblemDescription = r.ProblemDescription,
                Diagnosis = r.Diagnosis,
                CustomerName = r.Customer.FullName,
                TechnicianName = r.Technician != null ? r.Technician.FullName : null,
                Status = r.Status,
                LaborCost = r.LaborCost,
                PartsCost = r.RepairParts.Sum(rp =>
                    (decimal?)(rp.Quantity * rp.UnitPriceAtUse)) ?? 0m,
                RepairPartsCount = r.RepairParts.Count,
                CreatedOn = r.CreatedOn,
                CompletedOn = r.CompletedOn
            })
            .FirstOrDefaultAsync();
    }

    public async Task<RepairRequestFormViewModel?> GetForEditAsync(int id)
    {
        return await _context.RepairRequests
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RepairRequestFormViewModel
            {
                CustomerId = r.CustomerId,
                TechnicianId = r.TechnicianId,
                DeviceName = r.DeviceName,
                ProblemDescription = r.ProblemDescription,
                Diagnosis = r.Diagnosis,
                Status = r.Status,
                LaborCost = r.LaborCost
            })
            .FirstOrDefaultAsync();
    }

    public async Task PopulateDropdownsAsync(
        RepairRequestFormViewModel model,
        int? repairId = null)
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .ThenBy(c => c.Id)
            .Select(c => new { c.Id, c.FullName })
            .ToListAsync();

        model.Customers = customers
            .Select(c => new SelectListItem(c.FullName, c.Id.ToString()))
            .ToList();

        int existingRepairId = repairId ?? 0;
        var technicians = await _context.Technicians
            .AsNoTracking()
            .Where(t => t.IsActive ||
                t.RepairRequests.Any(r => r.Id == existingRepairId))
            .OrderBy(t => t.FullName)
            .ThenBy(t => t.Id)
            .Select(t => new { t.Id, t.FullName, t.IsActive })
            .ToListAsync();

        model.Technicians = technicians
            .Select(t => new SelectListItem(
                t.IsActive ? t.FullName : t.FullName + " (Inactive)",
                t.Id.ToString()))
            .ToList();
    }

    public async Task<(RepairRequestSaveResult Result, int? Id)> CreateAsync(
        RepairRequestFormViewModel model)
    {
        var validation = await ValidateReferencesAsync(model.CustomerId, model.TechnicianId);
        if (validation != RepairRequestSaveResult.Success)
        {
            return (validation, null);
        }

        var repair = new RepairRequest
        {
            CustomerId = model.CustomerId,
            TechnicianId = model.TechnicianId,
            DeviceName = model.DeviceName.Trim(),
            ProblemDescription = model.ProblemDescription.Trim(),
            Diagnosis = NormalizeOptionalText(model.Diagnosis),
            LaborCost = model.LaborCost,
            Status = RepairStatus.New,
            CreatedOn = DateTime.UtcNow
        };

        _context.RepairRequests.Add(repair);

        try
        {
            await _context.SaveChangesAsync();
            return (RepairRequestSaveResult.Success, repair.Id);
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            var currentValidation = await ValidateReferencesAsync(model.CustomerId, model.TechnicianId);
            if (currentValidation != RepairRequestSaveResult.Success)
            {
                return (currentValidation, null);
            }

            throw;
        }
    }

    public async Task<RepairRequestSaveResult> UpdateAsync(
        int id,
        RepairRequestFormViewModel model)
    {
        var repair = await _context.RepairRequests.FindAsync(id);
        if (repair is null)
        {
            return RepairRequestSaveResult.NotFound;
        }

        var validation = await ValidateReferencesAsync(
            model.CustomerId, model.TechnicianId, repair.TechnicianId);

        if (validation != RepairRequestSaveResult.Success)
        {
            return validation;
        }

        int? previousTechnicianId = repair.TechnicianId;

        repair.CustomerId = model.CustomerId;
        repair.TechnicianId = model.TechnicianId;
        repair.DeviceName = model.DeviceName.Trim();
        repair.ProblemDescription = model.ProblemDescription.Trim();
        repair.Diagnosis = NormalizeOptionalText(model.Diagnosis);
        repair.LaborCost = model.LaborCost;

        try
        {
            await _context.SaveChangesAsync();
            return RepairRequestSaveResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return RepairRequestSaveResult.NotFound;
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            var currentValidation = await ValidateReferencesAsync(
                model.CustomerId, model.TechnicianId, previousTechnicianId);
            if (currentValidation != RepairRequestSaveResult.Success)
            {
                return currentValidation;
            }

            throw;
        }
    }

    public async Task<RepairRequestDeleteResult> DeleteAsync(int id)
    {

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var repair = await _context.RepairRequests.FindAsync(id);
        if (repair is null)
        {
            return RepairRequestDeleteResult.NotFound;
        }

        bool hasParts = await _context.RepairParts
            .AnyAsync(rp => rp.RepairRequestId == id);

        if (hasParts)
        {
            return RepairRequestDeleteResult.HasRepairParts;
        }

        _context.RepairRequests.Remove(repair);

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return RepairRequestDeleteResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return RepairRequestDeleteResult.NotFound;
        }
    }

    private async Task<RepairRequestSaveResult> ValidateReferencesAsync(
        int customerId,
        int? technicianId,
        int? previousTechnicianId = null)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == customerId))
        {
            return RepairRequestSaveResult.InvalidCustomer;
        }

        if (!technicianId.HasValue)
        {
            return RepairRequestSaveResult.Success;
        }

        var technician = await _context.Technicians
            .AsNoTracking()
            .Where(t => t.Id == technicianId.Value)
            .Select(t => new { t.IsActive })
            .FirstOrDefaultAsync();

        if (technician is null)
        {
            return RepairRequestSaveResult.InvalidTechnician;
        }

        if (!technician.IsActive && previousTechnicianId != technicianId)
        {
            return RepairRequestSaveResult.InactiveTechnician;
        }

        return RepairRequestSaveResult.Success;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsForeignKeyViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException { Number: 547 };
    }
}
