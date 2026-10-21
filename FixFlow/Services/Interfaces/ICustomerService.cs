using FixFlow.ViewModels.Customers;

namespace FixFlow.Services.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerListViewModel>> GetAllAsync();

    Task<CustomerDetailsViewModel?> GetDetailsAsync(int id);

    Task<CustomerFormViewModel?> GetForEditAsync(int id);

    Task<int> CreateAsync(CustomerFormViewModel model);

    Task<bool> UpdateAsync(int id, CustomerFormViewModel model);

    Task<CustomerDeleteResult> DeleteAsync(int id);
}