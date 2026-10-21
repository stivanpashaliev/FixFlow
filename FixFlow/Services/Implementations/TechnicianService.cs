using FixFlow.Models.Entities;
using FixFlow.Services.Interfaces;
using FixFlow.ViewModels.Technicians;
using FixFlow.Web.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Services.Implementations;

public class TechnicianService : ITechnicianService
{
    private readonly FixFlowDbContext _context;

    public TechnicianService(FixFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TechnicianListViewModel>> GetAllAsync()
    {
        return await _context.Technicians
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .ThenBy(t => t.Id)
            .Select(t => new TechnicianListViewModel
            {
                Id = t.Id,
                FullName = t.FullName,
                Specialization = t.Specialization,
                IsActive = t.IsActive,
                RepairRequestsCount = t.RepairRequests.Count
            })
            .ToListAsync();
    }

    public async Task<TechnicianDetailsViewModel?> GetDetailsAsync(int id)
    {
        return await _context.Technicians
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TechnicianDetailsViewModel
            {
                Id = t.Id,
                FullName = t.FullName,
                Specialization = t.Specialization,
                IsActive = t.IsActive,
                RepairRequestsCount = t.RepairRequests.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TechnicianFormViewModel?> GetForEditAsync(int id)
    {
        return await _context.Technicians
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TechnicianFormViewModel
            {
                FullName = t.FullName,
                Specialization = t.Specialization,
                IsActive = t.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(TechnicianFormViewModel model)
    {
        var technician = new Technician
        {
            FullName = model.FullName.Trim(),
            Specialization = NormalizeSpecialization(model.Specialization),
            IsActive = model.IsActive
        };

        _context.Technicians.Add(technician);
        await _context.SaveChangesAsync();

        return technician.Id;
    }

    public async Task<bool> UpdateAsync(int id, TechnicianFormViewModel model)
    {
        var technician = await _context.Technicians.FindAsync(id);
        if (technician is null)
        {
            return false;
        }

        technician.FullName = model.FullName.Trim();
        technician.Specialization = NormalizeSpecialization(model.Specialization);
        technician.IsActive = model.IsActive;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public async Task<TechnicianDeleteResult> DeleteAsync(int id)
    {
        var technician = await _context.Technicians.FindAsync(id);
        if (technician is null)
        {
            return TechnicianDeleteResult.NotFound;
        }

        bool hasRepairs = await _context.RepairRequests
            .AnyAsync(r => r.TechnicianId == id);

        if (hasRepairs)
        {
            return TechnicianDeleteResult.HasRepairRequests;
        }

        _context.Technicians.Remove(technician);

        try
        {
            await _context.SaveChangesAsync();
            return TechnicianDeleteResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return TechnicianDeleteResult.NotFound;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException { Number: 547 })
        {
            return TechnicianDeleteResult.HasRepairRequests;
        }
    }

    private static string? NormalizeSpecialization(string? specialization)
    {
        return string.IsNullOrWhiteSpace(specialization)
            ? null
            : specialization.Trim();
    }
}