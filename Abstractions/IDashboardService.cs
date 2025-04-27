using finalProject.ViewModels;

namespace finalProject.Abstractions
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(string userId);
        Task<bool> AddAccountAsync(string userId, DashboardViewModel model);
        Task<bool> DeleteAccountAsync(string userId, int accountId);
    }
}
