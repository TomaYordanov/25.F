using finalProject.Data;
using finalProject.Models;
using finalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace finalProject.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var totalSpent = _context.Transactions.Sum(t => t.Amount);
            var transactionCount = _context.Transactions.Count();
            var categoryCount = _context.Categories.Count();

            ViewBag.TotalSpent = totalSpent;
            ViewBag.TransactionCount = transactionCount;
            ViewBag.CategoryCount = categoryCount;

            return View(new DashboardViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddAccount(DashboardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid account details.";
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);

            var totalProceeds = _context.Transactions
                .Where(t => t.UserId == userId && t.Amount > 0)
                .Sum(t => t.Amount);

            var totalPayments = _context.Transactions
                .Where(t => t.UserId == userId && t.Amount < 0)
                .Sum(t => t.Amount);

            var availableBalance = totalProceeds + totalPayments; 

            if (model.NewAccountBalance > availableBalance)
            {
                TempData["Error"] = "You cannot assign more balance than you currently have.";
                return RedirectToAction("Index");
            }

            var account = new Account
            {
                Name = model.NewAccountName.Trim(),
                Balance = model.NewAccountBalance,
                UserId = userId
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account created successfully!";
            return RedirectToAction("Index");
        }

    }
}
