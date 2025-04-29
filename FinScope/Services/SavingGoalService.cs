using FinScope.Abstractions;
using FinScope.Data;
using FinScope.Models;
using Microsoft.EntityFrameworkCore;

namespace FinScope.Services
{
    public class SavingGoalService : ISavingGoalService
    {
        private readonly ApplicationDbContext _context;

        public SavingGoalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddGoalAsync(SavingGoal goal)
        {
            if (string.IsNullOrEmpty(goal.UserId))
            {
                throw new ArgumentException("UserId must be set before adding a SavingGoal.");
            }

            goal.IsCompleted = goal.CurrentAmount >= goal.TargetAmount;
            _context.SavingGoals.Add(goal);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<SavingGoal>> GetGoalsAsync(string userId)
        {
            return await _context.SavingGoals
                .Where(g => g.UserId == userId)
                .ToListAsync();
        }

        public async Task<SavingGoal> GetGoalByIdAsync(int id, string userId)
        {
            return await _context.SavingGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
        }

        public async Task UpdateGoalAsync(SavingGoal goal)
        {
            goal.IsCompleted = goal.CurrentAmount >= goal.TargetAmount;
            _context.SavingGoals.Update(goal);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGoalAsync(int id, string userId)
        {
            var goal = await _context.SavingGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
            if (goal != null)
            {
                _context.SavingGoals.Remove(goal);
                await _context.SaveChangesAsync();
            }
        }
    }

}
