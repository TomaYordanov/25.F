using finalProject.Abstractions;
using finalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace finalProject.Controllers
{
    [Authorize]
    public class VisualizationsController : Controller
    {
        private readonly ISavingGoalService _savingGoalService;
        private readonly UserManager<ApplicationUser> _userManager;

        public VisualizationsController(ISavingGoalService savingGoalService, UserManager<ApplicationUser> userManager)
        {
            _savingGoalService = savingGoalService;
            _userManager = userManager;
        }

        private async Task<string> GetUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        public async Task<IActionResult> YearlySavingsChart()
        {
            var userId = await GetUserIdAsync();
            var goals = await _savingGoalService.GetGoalsAsync(userId);

            var yearlySavings = goals
                .Where(g => g.Deadline.HasValue)
                .GroupBy(g => g.Deadline.Value.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalSaved = g.Sum(goal => goal.CurrentAmount)
                })
                .OrderBy(g => g.Year)
                .ToList();

            return View(yearlySavings);
        }
    }
}
