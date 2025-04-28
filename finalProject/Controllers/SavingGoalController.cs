using finalProject.Abstractions;
using finalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace finalProject.Controllers
{
    [Authorize]
    public class SavingGoalController : Controller
    {
        private readonly ISavingGoalService _savingGoalService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SavingGoalController(ISavingGoalService savingGoalService, UserManager<ApplicationUser> userManager)
        {
            _savingGoalService = savingGoalService;
            _userManager = userManager;
        }

        private async Task<string> GetUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        public async Task<IActionResult> Index()
        {
            var userId = await GetUserIdAsync();
            var goals = await _savingGoalService.GetGoalsAsync(userId);
            return View(goals);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavingGoal goal)
        {
            if (ModelState.ErrorCount <= 3)
            {
                goal.UserId = await GetUserIdAsync();
                Console.WriteLine($"Creating goal for user: {goal.UserId}");
                await _savingGoalService.AddGoalAsync(goal);
                return RedirectToAction(nameof(Index));
            }

            return View(goal);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = await GetUserIdAsync();
            var goal = await _savingGoalService.GetGoalByIdAsync(id, userId);
            if (goal == null) return NotFound();
            return View(goal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SavingGoal goal)
        {
            if (ModelState.ErrorCount <= 2)
            {
                goal.UserId = await GetUserIdAsync();
                await _savingGoalService.UpdateGoalAsync(goal);
                return RedirectToAction(nameof(Index));
            }
            return View(goal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = await GetUserIdAsync();
            await _savingGoalService.DeleteGoalAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }
    }
}
