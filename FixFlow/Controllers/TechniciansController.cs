using FixFlow.Services.Interfaces;
using FixFlow.ViewModels.Technicians;
using FixFlow.Web.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace FixFlow.Controllers;

public class TechniciansController : Controller
{
    private readonly ITechnicianService _technicianService;

    public TechniciansController(ITechnicianService technicianService)
    {
        _technicianService = technicianService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var technicians = await _technicianService.GetAllAsync();
        return View(technicians);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var technician = await _technicianService.GetDetailsAsync(id);
        return technician is null ? NotFound() : View(technician);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TechnicianFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TechnicianFormViewModel model)
    {
        ValidateTrimmedFullName(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        int id = await _technicianService.CreateAsync(model);
        TempData["SuccessMessage"] = "Technician created successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _technicianService.GetForEditAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TechnicianFormViewModel model)
    {
        ValidateTrimmedFullName(model);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool updated = await _technicianService.UpdateAsync(id, model);
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Technician updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var technician = await _technicianService.GetDetailsAsync(id);
        return technician is null ? NotFound() : View(technician);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _technicianService.DeleteAsync(id);

        switch (result)
        {
            case TechnicianDeleteResult.NotFound:
                return NotFound();

            case TechnicianDeleteResult.HasRepairRequests:
                TempData["ErrorMessage"] =
                    "This technician cannot be deleted because they have repair requests.";
                return RedirectToAction(nameof(Delete), new { id });

            case TechnicianDeleteResult.Success:
                TempData["SuccessMessage"] = "Technician deleted successfully.";
                return RedirectToAction(nameof(Index));

            default:
                return StatusCode(500);
        }
    }

    private void ValidateTrimmedFullName(TechnicianFormViewModel model)
    {
        if (ModelState.IsValid &&
            model.FullName.Trim().Length < ValidationConstants.Technician.FullNameMinLength)
        {
            ModelState.AddModelError(
                nameof(model.FullName),
                "Full name must be between 2 and 100 characters.");
        }
    }
}