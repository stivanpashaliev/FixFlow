using System.Data;
using FixFlow.Models.Entities;
using FixFlow.Models.Enums;
using FixFlow.Services.Interfaces;
using FixFlow.Common.Rules;
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
        var model = await _context.RepairRequests
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

        if (model is null)
        {
            return null;
        }

        model.StatusHistory = await _context.RepairStatusHistories
            .AsNoTracking()
            .Where(h => h.RepairRequestId == id)
            .OrderByDescending(h => h.ChangedOn)
            .ThenByDescending(h => h.Id)
            .Select(h => new RepairStatusHistoryItemViewModel
            {
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                ChangedOn = h.ChangedOn,
                Note = h.Note
            })
            .ToListAsync();

        return model;
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

    public async Task<ChangeRepairStatusViewModel?> GetForStatusChangeAsync(int id)
    {
        var model = await _context.RepairRequests
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ChangeRepairStatusViewModel
            {
                RepairRequestId = r.Id,
                DeviceName = r.DeviceName,
                CurrentStatus = r.Status,
                ExpectedStatus = r.Status
            })
            .FirstOrDefaultAsync();

        if (model is null)
        {
            return null;
        }

        model.AvailableStatuses = RepairStatusRules.GetAllowedNextStatuses(model.CurrentStatus)
            .Select(status => new SelectListItem(status.ToString(), ((int)status).ToString()))
            .ToList();

        return model;
    }

    public async Task<RepairStatusChangeResult> ChangeStatusAsync(
        int id,
        ChangeRepairStatusViewModel model)
    {
        // Only the URL id and the requested next status are trusted as input.
        // The current status and business facts always come from the database.
        if (model.Note?.Trim().Length > FixFlow.Web.Common.Constants.ValidationConstants
                .RepairStatusHistory.NoteMaxLength)
        {
            return RepairStatusChangeResult.InvalidNote;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var repair = await _context.RepairRequests
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Status,
                r.TechnicianId,
                r.Diagnosis
            })
            .FirstOrDefaultAsync();

        if (repair is null)
        {
            return RepairStatusChangeResult.NotFound;
        }

        if (!model.ExpectedStatus.HasValue || model.ExpectedStatus.Value != repair.Status)
        {
            return RepairStatusChangeResult.ConcurrentModification;
        }

        if (!model.NewStatus.HasValue ||
            !RepairStatusRules.IsAllowed(repair.Status, model.NewStatus.Value))
        {
            return RepairStatusChangeResult.InvalidTransition;
        }

        var nextStatus = model.NewStatus.Value;
        var note = NormalizeOptionalText(model.Note);

        if (nextStatus == RepairStatus.Cancelled && note is null)
        {
            return RepairStatusChangeResult.CancellationNoteRequired;
        }

        if ((nextStatus is RepairStatus.InProgress or RepairStatus.Completed) &&
            !repair.TechnicianId.HasValue)
        {
            return RepairStatusChangeResult.MissingTechnician;
        }

        if (nextStatus == RepairStatus.Completed && string.IsNullOrWhiteSpace(repair.Diagnosis))
        {
            return RepairStatusChangeResult.MissingDiagnosis;
        }

        if (nextStatus == RepairStatus.Cancelled &&
            await _context.RepairParts.AnyAsync(rp => rp.RepairRequestId == id))
        {
            return RepairStatusChangeResult.CannotCancelWithParts;
        }

        var now = DateTime.UtcNow;
        var update = _context.RepairRequests
            .Where(r => r.Id == id && r.Status == repair.Status);

        if (nextStatus is RepairStatus.InProgress or RepairStatus.Completed)
        {
            update = update.Where(r => r.TechnicianId != null);
        }

        if (nextStatus == RepairStatus.Completed)
        {
            update = update.Where(r => r.Diagnosis != null && r.Diagnosis.Trim() != "");
        }

        if (nextStatus == RepairStatus.Cancelled)
        {
            update = update.Where(r => !r.RepairParts.Any());
        }

        int updatedRows;
        if (nextStatus == RepairStatus.Completed)
        {
            updatedRows = await update.ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.Status, nextStatus)
                .SetProperty(r => r.CompletedOn, now));
        }
        else
        {
            updatedRows = await update.ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.Status, nextStatus));
        }

        if (updatedRows == 0)
        {
            bool stillExists = await _context.RepairRequests.AnyAsync(r => r.Id == id);
            return stillExists
                ? RepairStatusChangeResult.ConcurrentModification
                : RepairStatusChangeResult.NotFound;
        }

        _context.RepairStatusHistories.Add(new RepairStatusHistory
        {
            RepairRequestId = id,
            OldStatus = repair.Status,
            NewStatus = nextStatus,
            ChangedOn = now,
            Note = note
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return RepairStatusChangeResult.Success;
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

        repair.StatusHistory.Add(new RepairStatusHistory
        {
            OldStatus = RepairStatus.New,
            NewStatus = RepairStatus.New,
            ChangedOn = repair.CreatedOn,
            Note = "Repair request registered."
        });

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
        var repair = await _context.RepairRequests
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new { r.Status, r.TechnicianId })
            .FirstOrDefaultAsync();

        if (repair is null)
        {
            return RepairRequestSaveResult.NotFound;
        }

        if (!RepairStatusRules.CanEdit(repair.Status))
        {
            return RepairRequestSaveResult.ClosedForEditing;
        }

        if (repair.Status == RepairStatus.InProgress && !model.TechnicianId.HasValue)
        {
            return RepairRequestSaveResult.TechnicianRequiredForInProgress;
        }

        var validation = await ValidateReferencesAsync(
            model.CustomerId, model.TechnicianId, repair.TechnicianId);

        if (validation != RepairRequestSaveResult.Success)
        {
            return validation;
        }

        string deviceName = model.DeviceName.Trim();
        string problemDescription = model.ProblemDescription.Trim();
        string? diagnosis = NormalizeOptionalText(model.Diagnosis);

        try
        {

            int updatedRows = await _context.RepairRequests
                .Where(r => r.Id == id && r.Status == repair.Status)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.CustomerId, model.CustomerId)
                    .SetProperty(r => r.TechnicianId, model.TechnicianId)
                    .SetProperty(r => r.DeviceName, deviceName)
                    .SetProperty(r => r.ProblemDescription, problemDescription)
                    .SetProperty(r => r.Diagnosis, diagnosis)
                    .SetProperty(r => r.LaborCost, model.LaborCost));

            if (updatedRows == 0)
            {
                bool stillExists = await _context.RepairRequests.AnyAsync(r => r.Id == id);
                return stillExists
                    ? RepairRequestSaveResult.ConcurrentModification
                    : RepairRequestSaveResult.NotFound;
            }

            return RepairRequestSaveResult.Success;
        }
        catch (Exception ex) when (IsForeignKeyViolation(ex))
        {
            var currentValidation = await ValidateReferencesAsync(
                model.CustomerId, model.TechnicianId, repair.TechnicianId);
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

        if (!RepairStatusRules.CanDelete(repair.Status))
        {
            return RepairRequestDeleteResult.NotAllowedInCurrentStatus;
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

    private static bool IsForeignKeyViolation(Exception ex)
    {
        return ex is SqlException { Number: 547 } ||
            ex is DbUpdateException { InnerException: SqlException { Number: 547 } };
    }
}
