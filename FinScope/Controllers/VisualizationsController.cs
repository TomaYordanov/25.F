using FinScope.Abstractions;
using FinScope.Data;
using FinScope.Models;
using FinScope.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
[Authorize]
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
        if (!User.Identity.IsAuthenticated)
            return RedirectToAction("Login", "Account");

        var userId = await GetUserIdAsync();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var goals = await _savingGoalService.GetGoalsAsync(userId);

        var yearlySavings = goals
            .Where(g => g.Deadline.HasValue)
            .GroupBy(g => g.Deadline.Value.Year)
            .Select(g => new YearlySavingsViewModel
            {
                Year = g.Key,
                TotalSaved = g.Sum(goal => goal.CurrentAmount)
            })
            .OrderBy(g => g.Year)
            .ToList();

        return View(yearlySavings);
    }

    public async Task<IActionResult> SpendingByCategoryChart(string month = "All", string year = "All")
    {
        var userId = await GetUserIdAsync(); 
        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); 
        }

        var transactions = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId) 
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

        var data = await transactions
            .Where(t => t.Amount < 0)
            .GroupBy(t => t.Category.Name)
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Sum(t => t.Amount)
            })
            .ToListAsync();

        return View(data);
    }

    public async Task<IActionResult> MonthlyBalanceGraph()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account");
        }

        var userId = await GetUserIdAsync();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var monthlyBalances = transactions
            .GroupBy(t => new { t.TransactionDateTime.Year, t.TransactionDateTime.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Income = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                Expenditure = g.Where(t => t.Amount < 0).Sum(t => t.Amount)
            })
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .ToList();

        return View(monthlyBalances);
    }
}



