using Microsoft.AspNetCore.Mvc;
using finalProject.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace finalProject.Controllers
{
    public class VisualizationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisualizationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> SpendingByCategoryChart(string month = "All", string year = "All")
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

            var grouped = await transactions
                .Where(t => t.Amount < 0)
                .GroupBy(t => t.Category.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            return View(grouped);
        }

        public async Task<IActionResult> MonthlyBalanceChart()
        {
            var transactions = await _context.Transactions
                .Where(t => t.TransactionDateTime != null)
                .ToListAsync();

            var monthlyData = transactions
                .GroupBy(t => new { t.TransactionDateTime.Year, t.TransactionDateTime.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Income = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                    Expenditure = g.Where(t => t.Amount < 0).Sum(t => t.Amount)
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToList();

            return View(monthlyData);
        }

       // public async Task<IActionResult> YearlySavingsChart()
       // {
      //  }


    }
}
