using finalProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace finalProject.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var transactions = _context.Transactions
                .Include(t => t.Account)
                .Include(t => t.Category)
                .ToList();
            return View(transactions);
        }
    }

}
