using FixFlow.Models.Entities;
using FixFlow.Services.Interfaces;
using FixFlow.ViewModels.Customers;
using FixFlow.Web.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FixFlow.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly FixFlowDbContext _context;

    public CustomerService(FixFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CustomerListViewModel>> GetAllAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .ThenBy(c => c.Id)
            .Select(c => new CustomerListViewModel
            {
                Id = c.Id,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                RepairRequestsCount = c.RepairRequests.Count
            })
            .ToListAsync();
    }

    public async Task<CustomerDetailsViewModel?> GetDetailsAsync(int id)
    {
        return await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerDetailsViewModel
            {
                Id = c.Id,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                CreatedOn = c.CreatedOn,
                RepairRequestsCount = c.RepairRequests.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerFormViewModel?> GetForEditAsync(int id)
    {
        return await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerFormViewModel
            {
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(CustomerFormViewModel model)
    {
        var customer = new Customer
        {
            FullName = model.FullName.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            Email = NormalizeEmail(model.Email),
            CreatedOn = DateTime.UtcNow
        };

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        return customer.Id;
    }

    public async Task<bool> UpdateAsync(int id, CustomerFormViewModel model)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
        {
            return false;
        }

        customer.FullName = model.FullName.Trim();
        customer.PhoneNumber = model.PhoneNumber.Trim();
        customer.Email = NormalizeEmail(model.Email);

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

    public async Task<CustomerDeleteResult> DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
        {
            return CustomerDeleteResult.NotFound;
        }

        bool hasRepairs = await _context.RepairRequests
            .AnyAsync(r => r.CustomerId == id);

        if (hasRepairs)
        {
            return CustomerDeleteResult.HasRepairRequests;
        }

        _context.Customers.Remove(customer);

        try
        {
            await _context.SaveChangesAsync();
            return CustomerDeleteResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return CustomerDeleteResult.NotFound;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException { Number: 547 })
        {
            return CustomerDeleteResult.HasRepairRequests;
        }
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
}