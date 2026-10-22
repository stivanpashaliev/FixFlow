using FixFlow.Models.Enums;
using FixFlow.Services.Interfaces;
using FixFlow.Common.Rules;
using FixFlow.ViewModels.RepairRequests;
using FixFlow.Web.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace FixFlow.Controllers;

public class RepairRequestsController : Controller
{
    private readonly IRepairRequestService _repairRequestService;

    public RepairRequestsController(IRepairRequestService repairRequestService)
    {
        _repairRequestService = repairRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var repairs = await _repairRequestService.GetAllAsync();
        return View(repairs);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var repair = await _repairRequestService.GetDetailsAsync(id);
        return repair is null ? NotFound() : View(repair);
    }

    [HttpGet]
    public async Task<IActionResult> ChangeStatus(int id)
    {
        var model = await _repairRequestService.GetForStatusChangeAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        if (!RepairStatusRules.GetAllowedNextStatuses(model.CurrentStatus).Any())
        {
            TempData["ErrorMessage"] = "This repair has reached a final status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, ChangeRepairStatusViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await RedisplayChangeStatusAsync(id, model);
        }

        var result = await _repairRequestService.ChangeStatusAsync(id, model);
        switch (result)
        {
            case RepairStatusChangeResult.Success:
                TempData["SuccessMessage"] = "Repair status updated successfully.";
                return RedirectToAction(nameof(Details), new { id });

            case RepairStatusChangeResult.NotFound:
                return NotFound();

            case RepairStatusChangeResult.ConcurrentModification:
                TempData["ErrorMessage"] =
                    "The repair changed while you were editing. Review its current status and try again.";
                return RedirectToAction(nameof(Details), new { id });

            case RepairStatusChangeResult.InvalidTransition:
                ModelState.AddModelError(nameof(model.NewStatus),
                    "The selected status transition is not permitted.");
                break;

            case RepairStatusChangeResult.MissingTechnician:
                ModelState.AddModelError(string.Empty,
                    "Assign a technician before starting or completing work.");
                break;

            case RepairStatusChangeResult.MissingDiagnosis:
                ModelState.AddModelError(string.Empty,
                    "Enter a diagnosis before completing the repair.");
                break;

            case RepairStatusChangeResult.CancellationNoteRequired:
                ModelState.AddModelError(nameof(model.Note),
                    "A cancellation reason is required.");
                break;

            case RepairStatusChangeResult.CannotCancelWithParts:
                ModelState.AddModelError(string.Empty,
                    "This repair uses spare parts. Cancellation is blocked until inventory return is implemented.");
                break;

            case RepairStatusChangeResult.InvalidNote:
                ModelState.AddModelError(nameof(model.Note),
                    "Note cannot exceed 500 characters.");
                break;

            default:
                return StatusCode(500);
        }

        return await RedisplayChangeStatusAsync(id, model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new RepairRequestFormViewModel();
        await _repairRequestService.PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RepairRequestFormViewModel model)
    {
        ValidateForm(model);
        if (!ModelState.IsValid)
        {
            await _repairRequestService.PopulateDropdownsAsync(model);
            return View(model);
        }

        var (result, id) = await _repairRequestService.CreateAsync(model);
        if (result != RepairRequestSaveResult.Success)
        {
            AddReferenceError(result);
            await _repairRequestService.PopulateDropdownsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Repair request created successfully.";
        return RedirectToAction(nameof(Details), new { id = id!.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _repairRequestService.GetForEditAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        if (!RepairStatusRules.CanEdit(model.Status))
        {
            TempData["ErrorMessage"] = "Completed, delivered or cancelled repairs cannot be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _repairRequestService.PopulateDropdownsAsync(model, id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RepairRequestFormViewModel model)
    {
        ValidateForm(model);
        if (!ModelState.IsValid)
        {
            return await RedisplayEditAsync(id, model);
        }

        var result = await _repairRequestService.UpdateAsync(id, model);
        if (result == RepairRequestSaveResult.NotFound)
        {
            return NotFound();
        }

        if (result == RepairRequestSaveResult.ClosedForEditing ||
            result == RepairRequestSaveResult.ConcurrentModification)
        {
            TempData["ErrorMessage"] = "The repair is no longer editable. Review its current status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (result != RepairRequestSaveResult.Success)
        {
            AddReferenceError(result);
            return await RedisplayEditAsync(id, model);
        }

        TempData["SuccessMessage"] = "Repair request updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var repair = await _repairRequestService.GetDetailsAsync(id);
        if (repair is null)
        {
            return NotFound();
        }

        if (!repair.CanDelete)
        {
            TempData["ErrorMessage"] = "Only new repairs can be deleted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(repair);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _repairRequestService.DeleteAsync(id);

        switch (result)
        {
            case RepairRequestDeleteResult.NotFound:
                return NotFound();

            case RepairRequestDeleteResult.NotAllowedInCurrentStatus:
                TempData["ErrorMessage"] = "Only new repairs can be deleted.";
                return RedirectToAction(nameof(Details), new { id });

            case RepairRequestDeleteResult.HasRepairParts:
                TempData["ErrorMessage"] =
                    "This repair request cannot be deleted because spare parts have been used. " +
                    "Stock restoration is not implemented yet.";
                return RedirectToAction(nameof(Delete), new { id });

            case RepairRequestDeleteResult.Success:
                TempData["SuccessMessage"] = "Repair request deleted successfully.";
                return RedirectToAction(nameof(Index));

            default:
                return StatusCode(500);
        }
    }

    private async Task<IActionResult> RedisplayChangeStatusAsync(
        int id,
        ChangeRepairStatusViewModel submitted)
    {
        var current = await _repairRequestService.GetForStatusChangeAsync(id);
        if (current is null)
        {
            return NotFound();
        }

        if (!RepairStatusRules.GetAllowedNextStatuses(current.CurrentStatus).Any())
        {
            TempData["ErrorMessage"] = "This repair has reached a final status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (submitted.ExpectedStatus != current.CurrentStatus)
        {
            TempData["ErrorMessage"] =
                "The repair status has changed. Review the details and open a new form.";
            return RedirectToAction(nameof(Details), new { id });
        }

        current.NewStatus = submitted.NewStatus;
        current.Note = submitted.Note;
        return View("ChangeStatus", current);
    }

    private async Task<IActionResult> RedisplayEditAsync(
        int id,
        RepairRequestFormViewModel model)
    {
        var existing = await _repairRequestService.GetForEditAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        if (!RepairStatusRules.CanEdit(existing.Status))
        {
            TempData["ErrorMessage"] = "The repair is no longer editable.";
            return RedirectToAction(nameof(Details), new { id });
        }

        model.Status = existing.Status;
        await _repairRequestService.PopulateDropdownsAsync(model, id);
        return View("Edit", model);
    }

    private void ValidateForm(RepairRequestFormViewModel model)
    {
        if (model.DeviceName is not null &&
            model.DeviceName.Trim().Length < ValidationConstants.RepairRequest.DeviceNameMinLength)
        {
            ModelState.AddModelError(
                nameof(model.DeviceName), "Device name must be at least 2 characters long.");
        }

        if (model.ProblemDescription is not null &&
            model.ProblemDescription.Trim().Length < ValidationConstants.RepairRequest.ProblemDescriptionMinLength)
        {
            ModelState.AddModelError(
                nameof(model.ProblemDescription),
                "Problem description must be at least 10 characters long.");
        }

        if (decimal.Round(model.LaborCost, 2) != model.LaborCost)
        {
            ModelState.AddModelError(
                nameof(model.LaborCost), "Labor cost cannot have more than two decimal places.");
        }
    }

    private void AddReferenceError(RepairRequestSaveResult result)
    {
        switch (result)
        {
            case RepairRequestSaveResult.InvalidCustomer:
                ModelState.AddModelError(
                    nameof(RepairRequestFormViewModel.CustomerId),
                    "Please select an existing customer.");
                break;

            case RepairRequestSaveResult.InvalidTechnician:
                ModelState.AddModelError(
                    nameof(RepairRequestFormViewModel.TechnicianId),
                    "Please select an existing technician.");
                break;

            case RepairRequestSaveResult.InactiveTechnician:
                ModelState.AddModelError(
                    nameof(RepairRequestFormViewModel.TechnicianId),
                    "Only active technicians can receive new assignments.");
                break;
            case RepairRequestSaveResult.TechnicianRequiredForInProgress:
                ModelState.AddModelError(
                    nameof(RepairRequestFormViewModel.TechnicianId),
                    "An in-progress repair must have an assigned technician.");
                break;
        }
    }
}
