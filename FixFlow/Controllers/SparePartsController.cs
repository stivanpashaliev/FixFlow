using FixFlow.Services.Interfaces;
using FixFlow.ViewModels.SpareParts;
using Microsoft.AspNetCore.Mvc;

namespace FixFlow.Controllers;

public class SparePartsController : Controller
{
    private readonly ISparePartService _sparePartService;

    public SparePartsController(ISparePartService sparePartService)
    {
        _sparePartService = sparePartService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var parts = await _sparePartService.GetAllAsync();
        return View(parts);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var part = await _sparePartService.GetDetailsAsync(id);
        return part is null ? NotFound() : View(part);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new SparePartFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SparePartFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        int? id = await _sparePartService.CreateAsync(model);
        if (id is null)
        {
            ModelState.AddModelError(nameof(model.SKU), "This SKU is already in use.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Spare part created successfully.";
        return RedirectToAction(nameof(Details), new { id = id.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _sparePartService.GetForEditAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SparePartFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Invalid input must not mask a missing record.
            var existing = await _sparePartService.GetForEditAsync(id);
            if (existing is null)
            {
                return NotFound();
            }

            return View(model);
        }

        var result = await _sparePartService.UpdateAsync(id, model);
        switch (result)
        {
            case SparePartUpdateResult.NotFound:
                return NotFound();

            case SparePartUpdateResult.DuplicateSku:
                ModelState.AddModelError(nameof(model.SKU), "This SKU is already in use.");
                return View(model);

            case SparePartUpdateResult.Success:
                TempData["SuccessMessage"] = "Spare part updated successfully.";
                return RedirectToAction(nameof(Details), new { id });

            default:
                return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var part = await _sparePartService.GetDetailsAsync(id);
        return part is null ? NotFound() : View(part);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _sparePartService.DeleteAsync(id);
        switch (result)
        {
            case SparePartDeleteResult.NotFound:
                return NotFound();

            case SparePartDeleteResult.UsedInRepairs:
                TempData["ErrorMessage"] =
                    "This spare part cannot be deleted because it is used in repair requests.";
                return RedirectToAction(nameof(Delete), new { id });

            case SparePartDeleteResult.Success:
                TempData["SuccessMessage"] = "Spare part deleted successfully.";
                return RedirectToAction(nameof(Index));

            default:
                return StatusCode(500);
        }
    }
}
