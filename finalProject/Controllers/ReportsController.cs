using finalProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace finalProject.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult SpendingByCategory(string month = "All", string year = "All")
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

        public async Task<IActionResult> MonthlyBalance()
        {
            var data = await _context.Transactions
                .GroupBy(t => new { t.TransactionDateTime.Year, t.TransactionDateTime.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Income = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                    Expenditure = g.Where(t => t.Amount < 0).Sum(t => t.Amount),
                    Balance = g.Sum(t => t.Amount)
                })
                .OrderByDescending(g => g.Year)
                .ThenByDescending(g => g.Month)
                .ToListAsync();

            return View(data);
        }


    }
}
