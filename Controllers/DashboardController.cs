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

            var mainAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Name == "Main");

            if (mainAccount == null)
            {
                TempData["Error"] = "Main account not found.";
                return RedirectToAction("Index");
            }

            if (model.NewAccountBalance > mainAccount.Balance)
            {
                TempData["Error"] = "You do not have enough funds in the Main account.";
                return RedirectToAction("Index");
            }

            // Create new account
            var newAccount = new Account
            {
                Name = model.NewAccountName.Trim(),
                Balance = model.NewAccountBalance,
                UserId = userId
            };

            
            mainAccount.Balance -= model.NewAccountBalance;

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account created successfully!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            var userId = _userManager.GetUserId(User);

            var accountToDelete = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (accountToDelete == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index");
            }

            if (accountToDelete.Name == "Main")
            {
                TempData["Error"] = "You cannot delete the default Main account.";
                return RedirectToAction("Index");
            }

            var mainAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Name == "Main");

            if (mainAccount == null)
            {
                TempData["Error"] = "Main account not found.";
                return RedirectToAction("Index");
            }

            // Transfer account balance back to Main
            mainAccount.Balance += accountToDelete.Balance;

            _context.Accounts.Remove(accountToDelete);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account deleted successfully and balance moved to Main.";
            return RedirectToAction("Index");
        }


    }
}
