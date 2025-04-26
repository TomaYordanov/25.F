using finalProject.Data;
using finalProject.Models;
using finalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var totalSpent = transactions
                .Where(t => t.Amount < 0)
                .Sum(t => t.Amount);

            var transactionCount = transactions.Count;
            var categoryCount = await _context.Categories.CountAsync();

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var totalBalance = accounts.Sum(a => a.Balance);

            // Create a default account if none exist
            if (!accounts.Any())
            {
                var defaultAccount = new Account
                {
                    Name = "Main",
                    Balance = 0,
                    UserId = userId
                };

                _context.Accounts.Add(defaultAccount);
                await _context.SaveChangesAsync();

                accounts = await _context.Accounts
                    .Where(a => a.UserId == userId)
                    .ToListAsync();
            }

            ViewBag.TotalSpent = Math.Abs(totalSpent);
            ViewBag.TransactionCount = transactionCount;
            ViewBag.CategoryCount = categoryCount;

            return View(new DashboardViewModel
            {
                Accounts = accounts,
                TotalBalance = totalBalance
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

            var totalProceeds = await _context.Transactions
                .Where(t => t.UserId == userId && t.Amount > 0)
                .SumAsync(t => t.Amount);

            var totalPayments = await _context.Transactions
                .Where(t => t.UserId == userId && t.Amount < 0)
                .SumAsync(t => t.Amount);

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

            if (account.Name == "Main")
            {
                TempData["Error"] = "You cannot delete the default account.";
                return RedirectToAction("Index");
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account deleted.";
            return RedirectToAction("Index");
        }

    }
}
