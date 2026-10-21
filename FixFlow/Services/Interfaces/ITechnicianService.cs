using FixFlow.ViewModels.Technicians;

namespace FixFlow.Services.Interfaces;

public interface ITechnicianService
{
    Task<IReadOnlyList<TechnicianListViewModel>> GetAllAsync();

    Task<TechnicianDetailsViewModel?> GetDetailsAsync(int id);

    Task<TechnicianFormViewModel?> GetForEditAsync(int id);

    Task<int> CreateAsync(TechnicianFormViewModel model);

    Task<bool> UpdateAsync(int id, TechnicianFormViewModel model);

    Task<TechnicianDeleteResult> DeleteAsync(int id);
}