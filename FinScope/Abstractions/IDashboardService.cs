using FinScope.ViewModels;

namespace FinScope.Abstractions
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(string userId);
        Task<bool> AddAccountAsync(string userId, DashboardViewModel model);
        Task<bool> DeleteAccountAsync(string userId, int accountId);
    }
}
