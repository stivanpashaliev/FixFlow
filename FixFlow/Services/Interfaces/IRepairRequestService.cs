using FixFlow.Models.Enums;
using FixFlow.ViewModels.RepairRequests;

namespace FixFlow.Services.Interfaces;

public interface IRepairRequestService
{
    Task<IReadOnlyList<RepairRequestListViewModel>> GetAllAsync();

    Task<RepairRequestDetailsViewModel?> GetDetailsAsync(int id);

    Task<RepairRequestFormViewModel?> GetForEditAsync(int id);

    Task PopulateDropdownsAsync(RepairRequestFormViewModel model, int? repairId = null);

    Task<(RepairRequestSaveResult Result, int? Id)> CreateAsync(RepairRequestFormViewModel model);

    Task<RepairRequestSaveResult> UpdateAsync(int id, RepairRequestFormViewModel model);

    Task<RepairRequestDeleteResult> DeleteAsync(int id);
}
