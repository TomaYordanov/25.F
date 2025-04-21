using finalProject.Data;
using Microsoft.AspNetCore.Mvc;

namespace finalProject.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalSpent = _context.Transactions.Sum(t => t.Amount);
            var transactionCount = _context.Transactions.Count();
            var categoryCount = _context.Categories.Count();

            ViewBag.TotalSpent = totalSpent;
            ViewBag.TransactionCount = transactionCount;
            ViewBag.CategoryCount = categoryCount;

            return View();
        }
    }

}
