using FixFlow.Models.Enums;
using FixFlow.Services.Interfaces;
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
        return repair is null ? NotFound() : View(repair);
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

    private async Task<IActionResult> RedisplayEditAsync(
        int id,
        RepairRequestFormViewModel model)
    {
        var existing = await _repairRequestService.GetForEditAsync(id);
        if (existing is null)
        {
            return NotFound();
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
        }
    }
}
