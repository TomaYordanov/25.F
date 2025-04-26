using finalProject.Data;
using finalProject.Models;
using finalProject.Services;
using finalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace finalProject.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SpreadsheetImportService _SpreadsheetImportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(
            ApplicationDbContext context,
            SpreadsheetImportService SpreadsheetImportService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _SpreadsheetImportService = SpreadsheetImportService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var transactionVMs = transactions.Select(t => new TransactionViewModel
            {
                Id = t.Id,
                TransactionDateTime = t.TransactionDateTime,
                Amount = t.Amount,
                Description = t.Description,
                CategoryName = t.Category?.Name,
                AccountName = t.Account?.Name
            }).ToList();

            var totalBalance = await _context.Accounts
                .Where(a => a.UserId == userId)
                .SumAsync(a => a.Balance);

            var categories = await _context.Categories.ToListAsync();
            var accounts = await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();

            var model = new TransactionIndexViewModel
            {
                Transactions = transactionVMs,
                TotalBalance = totalBalance,
                Categories = categories,
                Accounts = accounts
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManualAdd(TransactionIndexViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == model.SelectedAccountId && a.UserId == userId);

            if (account == null)
            {
                TempData["Error"] = "Invalid account.";
                return RedirectToAction("Index");
            }

            Category category = null;

            if (!string.IsNullOrWhiteSpace(model.ManualCategoryName))
            {
                // Try to find if a category with the same name exists
                category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == model.ManualCategoryName.Trim().ToLower());

                if (category == null)
                {
                    // Create new category
                    category = new Category { Name = model.ManualCategoryName.Trim() };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Use selected category
                category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == model.SelectedCategoryId);
            }

            if (category == null)
            {
                TempData["Error"] = "Invalid category.";
                return RedirectToAction("Index");
            }

            var transaction = new Transaction
            {
                TransactionDateTime = model.ManualDateTime,
                Amount = model.ManualAmount,
                Description = model.ManualDescription,
                AccountId = account.Id,
                CategoryId = category.Id,
                UserId = userId
            };

            _context.Transactions.Add(transaction);


            account.Balance += model.ManualAmount;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Transaction added successfully!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid Excel file.";
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);

            // Ensure at least one account exists
            var defaultAccount = await _context.Accounts
                .Where(a => a.UserId == userId)
                .FirstOrDefaultAsync();

            if (defaultAccount == null)
            {
                defaultAccount = new Account
                {
                    Name = "Default Account",
                    Balance = 0,
                    UserId = userId
                };
                _context.Accounts.Add(defaultAccount);
                await _context.SaveChangesAsync();
            }

            var defaultCategory = await _context.Categories.FirstOrDefaultAsync();

            if (defaultCategory == null)
            {
                defaultCategory = new Category { Name = "Miscellaneous" };
                _context.Categories.Add(defaultCategory);
                await _context.SaveChangesAsync();
            }

            using var stream = file.OpenReadStream();
            await _SpreadsheetImportService.ImportTransactionsAsync(
                stream,
                userId,
                defaultCategoryId: defaultCategory.Id,
                defaultAccountId: defaultAccount.Id
            );

            TempData["Success"] = "Transactions imported successfully!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> MoveTransaction(int transactionId, int newAccountId)
        {
            var userId = _userManager.GetUserId(User);

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found.";
                return RedirectToAction("Index");
            }

            var newAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == newAccountId && a.UserId == userId);

            if (newAccount == null)
            {
                TempData["Error"] = "Target account not found.";
                return RedirectToAction("Index");
            }

            if (transaction.AccountId == newAccountId)
            {
                TempData["Info"] = "Transaction is already in the selected account.";
                return RedirectToAction("Index");
            }

            // Reverse from old account
            transaction.Account.Balance -= transaction.Amount;

            // Apply to new account
            newAccount.Balance += transaction.Amount;

            // Update transaction
            transaction.AccountId = newAccountId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Transaction moved successfully!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found.";
                return RedirectToAction("Index");
            }

            // Adjust the account balance
            if (transaction.Account != null)
            {
                transaction.Account.Balance -= transaction.Amount;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Transaction deleted successfully and balance updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TransferTransaction(int transactionId, int targetAccountId, decimal transferAmount)
        {
            if (transferAmount <= 0)
            {
                TempData["Error"] = "Transfer amount must be positive.";
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);

            var originalTransaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

            if (originalTransaction == null)
            {
                TempData["Error"] = "Original transaction not found.";
                return RedirectToAction("Index");
            }

            var targetAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == targetAccountId && a.UserId == userId);

            if (targetAccount == null)
            {
                TempData["Error"] = "Target account not found.";
                return RedirectToAction("Index");
            }

            // Check if transfer amount is larger than available amount
            if (Math.Abs(transferAmount) > Math.Abs(originalTransaction.Amount))
            {
                TempData["Error"] = "Transfer amount cannot be greater than the original transaction amount.";
                return RedirectToAction("Index");
            }

            // Create the new transaction (copying properties)
            var transferredTransaction = new Transaction
            {
                TransactionDateTime = DateTime.Now,
                Amount = originalTransaction.Amount < 0 ? -transferAmount : transferAmount,
                Description = $"Transfer from {originalTransaction.Account.Name}: {originalTransaction.Description}",
                AccountId = targetAccount.Id,
                CategoryId = originalTransaction.CategoryId,
                UserId = userId
            };
            _context.Transactions.Add(transferredTransaction);

            // Adjust balances
            if (originalTransaction.Amount < 0)
            {
                // Original is negative
                originalTransaction.Account.Balance -= transferAmount; 
                targetAccount.Balance -= transferAmount; 
            }
            else
            {
                // Original is positive
                originalTransaction.Account.Balance -= transferAmount;
                targetAccount.Balance += transferAmount;
            }

            // Adjust the original transaction's amount
            if (originalTransaction.Amount < 0)
            {
                originalTransaction.Amount += transferAmount; 
            }
            else
            {
                originalTransaction.Amount -= transferAmount;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Transfer completed successfully!";
            return RedirectToAction("Index");
        }



    }
}
