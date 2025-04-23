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

            var model = new TransactionIndexViewModel
            {
                Transactions = transactionVMs,
                TotalBalance = totalBalance
            };

            return View(model);
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

            var defaultAccount = await _context.Accounts
                .Where(a => a.UserId == userId)
                .FirstOrDefaultAsync();

            var defaultCategory = await _context.Categories.FirstOrDefaultAsync(); 

            if (defaultAccount == null || defaultCategory == null)
            {
                TempData["Error"] = "Please make sure you have at least one account and one category before importing.";
                return RedirectToAction("Index");
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
