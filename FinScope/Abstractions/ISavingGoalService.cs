using FinScope.Models;

namespace FinScope.Abstractions
{
    public interface ISavingGoalService
    {
        Task<IEnumerable<SavingGoal>> GetGoalsAsync(string userId);
        Task<SavingGoal> GetGoalByIdAsync(int id, string userId);
        Task AddGoalAsync(SavingGoal goal);
        Task UpdateGoalAsync(SavingGoal goal);
        Task DeleteGoalAsync(int id, string userId);
    }
}
