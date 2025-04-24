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
            var userId = _userManager.GetUserId(User);

            var totalSpent = _context.Transactions
                .Where(t => t.UserId == userId && t.Amount < 0)
                .Sum(t => t.Amount);

            var transactionCount = _context.Transactions.Count(t => t.UserId == userId);
            var categoryCount = _context.Categories.Count();

            ViewBag.TotalSpent = Math.Abs(totalSpent);
            ViewBag.TransactionCount = transactionCount;
            ViewBag.CategoryCount = categoryCount;

            var accounts = _context.Accounts
                .Where(a => a.UserId == userId)
                .ToList();

            return View(new DashboardViewModel
            {
                Accounts = accounts
            });
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
        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            var userId = _userManager.GetUserId(User);

            var account = _context.Accounts
                .FirstOrDefault(a => a.Id == accountId && a.UserId == userId);

            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index");
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account deleted.";
            return RedirectToAction("Index");
        }

    }
}
