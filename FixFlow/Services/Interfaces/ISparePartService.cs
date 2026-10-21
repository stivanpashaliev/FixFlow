using FixFlow.ViewModels.SpareParts;

namespace FixFlow.Services.Interfaces;

public interface ISparePartService
{
    Task<IReadOnlyList<SparePartListViewModel>> GetAllAsync();

    Task<SparePartDetailsViewModel?> GetDetailsAsync(int id);

    Task<SparePartFormViewModel?> GetForEditAsync(int id);

    // Returns null when the SKU is already in use.
    Task<int?> CreateAsync(SparePartFormViewModel model);

    Task<SparePartUpdateResult> UpdateAsync(int id, SparePartFormViewModel model);

    Task<SparePartDeleteResult> DeleteAsync(int id);
}
