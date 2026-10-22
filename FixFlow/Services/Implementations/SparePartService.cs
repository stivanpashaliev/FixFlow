using FixFlow.Models.Entities;
using FixFlow.Services.Interfaces;
using FixFlow.ViewModels.SpareParts;
using FixFlow.Web.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Services.Implementations;

public class SparePartService : ISparePartService
{
    private readonly FixFlowDbContext _context;

    public SparePartService(FixFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SparePartListViewModel>> GetAllAsync()
    {
        return await _context.SpareParts
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Select(p => new SparePartListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                UnitPrice = p.UnitPrice,
                QuantityInStock = p.QuantityInStock
            })
            .ToListAsync();
    }

    public async Task<SparePartDetailsViewModel?> GetDetailsAsync(int id)
    {
        return await _context.SpareParts
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new SparePartDetailsViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                UnitPrice = p.UnitPrice,
                QuantityInStock = p.QuantityInStock,
                RepairRequestsCount = p.RepairParts.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SparePartFormViewModel?> GetForEditAsync(int id)
    {
        return await _context.SpareParts
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new SparePartFormViewModel
            {
                Name = p.Name,
                SKU = p.SKU,
                UnitPrice = p.UnitPrice,
                QuantityInStock = p.QuantityInStock
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> CreateAsync(SparePartFormViewModel model)
    {
        string sku = model.SKU.Trim();

        if (await _context.SpareParts.AnyAsync(p => p.SKU == sku))
        {
            return null;
        }

        var part = new SparePart
        {
            Name = model.Name.Trim(),
            SKU = sku,
            UnitPrice = model.UnitPrice,
            QuantityInStock = model.QuantityInStock
        };

        _context.SpareParts.Add(part);

        try
        {
            await _context.SaveChangesAsync();
            return part.Id;
        }
        catch (DbUpdateException ex) when (IsDuplicateSkuViolation(ex))
        {
            return null;
        }
    }

    public async Task<SparePartUpdateResult> UpdateAsync(int id, SparePartFormViewModel model)
    {
        var part = await _context.SpareParts.FindAsync(id);
        if (part is null)
        {
            return SparePartUpdateResult.NotFound;
        }

        string sku = model.SKU.Trim();
        bool duplicateSku = await _context.SpareParts
            .AnyAsync(p => p.Id != id && p.SKU == sku);

        if (duplicateSku)
        {
            return SparePartUpdateResult.DuplicateSku;
        }

        part.Name = model.Name.Trim();
        part.SKU = sku;
        part.UnitPrice = model.UnitPrice;
        part.QuantityInStock = model.QuantityInStock;

        try
        {
            await _context.SaveChangesAsync();
            return SparePartUpdateResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return SparePartUpdateResult.NotFound;
        }
        catch (DbUpdateException ex) when (IsDuplicateSkuViolation(ex))
        {
            return SparePartUpdateResult.DuplicateSku;
        }
    }

    public async Task<SparePartDeleteResult> DeleteAsync(int id)
    {
        var part = await _context.SpareParts.FindAsync(id);
        if (part is null)
        {
            return SparePartDeleteResult.NotFound;
        }

        bool usedInRepairs = await _context.RepairParts
            .AnyAsync(rp => rp.SparePartId == id);

        if (usedInRepairs)
        {
            return SparePartDeleteResult.UsedInRepairs;
        }

        _context.SpareParts.Remove(part);

        try
        {
            await _context.SaveChangesAsync();
            return SparePartDeleteResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return SparePartDeleteResult.NotFound;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            return SparePartDeleteResult.UsedInRepairs;
        }
    }

    private static bool IsDuplicateSkuViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
