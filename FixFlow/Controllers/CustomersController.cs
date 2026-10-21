using FixFlow.Services.Interfaces;
using FixFlow.ViewModels.Customers;
using Microsoft.AspNetCore.Mvc;

namespace FixFlow.Controllers;

public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var customers = await _customerService.GetAllAsync();
        return View(customers);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _customerService.GetDetailsAsync(id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CustomerFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        int id = await _customerService.CreateAsync(model);
        TempData["SuccessMessage"] = "Customer created successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _customerService.GetForEditAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool updated = await _customerService.UpdateAsync(id, model);
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Customer updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _customerService.GetDetailsAsync(id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _customerService.DeleteAsync(id);

        switch (result)
        {
            case CustomerDeleteResult.NotFound:
                return NotFound();

            case CustomerDeleteResult.HasRepairRequests:
                TempData["ErrorMessage"] =
                    "This customer cannot be deleted because they have repair requests.";
                return RedirectToAction(nameof(Delete), new { id });

            case CustomerDeleteResult.Success:
                TempData["SuccessMessage"] = "Customer deleted successfully.";
                return RedirectToAction(nameof(Index));

            default:
                return StatusCode(500);
        }
    }
}