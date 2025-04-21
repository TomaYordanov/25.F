using finalProject.Data;
using Microsoft.AspNetCore.Mvc;

namespace finalProject.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult SpendingByCategory()
        {
            var data = _context.Transactions
                .GroupBy(t => t.Category.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount)
                }).ToList();

            return View(data);
        }
    }

}
