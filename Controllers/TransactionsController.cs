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

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == model.SelectedAccountId && a.UserId == userId);
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == model.SelectedCategoryId);

            if (account == null || category == null)
            {
                TempData["Error"] = "Invalid account or category.";
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


    }
}
