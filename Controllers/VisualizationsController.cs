using finalProject.Abstractions;
using finalProject.Data;
using finalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class VisualizationsController : Controller
{
    private readonly ISavingGoalService _savingGoalService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context; 
    public VisualizationsController(ISavingGoalService savingGoalService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _savingGoalService = savingGoalService;
        _userManager = userManager;
        _context = context; 
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

    public IActionResult SpendingByCategoryChart(string month = "All", string year = "All")
    {
        var transactions = _context.Transactions
            .Include(t => t.Category)
            .AsQueryable();

        if (int.TryParse(month, out int monthNumber) && monthNumber >= 1 && monthNumber <= 12)
        {
            transactions = transactions
                .Where(t => t.TransactionDateTime.Month == monthNumber);
        }

        if (int.TryParse(year, out int yearNumber))
        {
            transactions = transactions
                .Where(t => t.TransactionDateTime.Year == yearNumber);
        }

        var data = transactions
            .Where(t => t.Amount < 0)
            .GroupBy(t => t.Category.Name)
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Sum(t => t.Amount)
            })
            .ToList();

        return View(data);
    }
}
