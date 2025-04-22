using finalProject.Data;
using finalProject.Models;
using finalProject.Services;
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
        private readonly ExcelImportService _excelImportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(
            ApplicationDbContext context,
            ExcelImportService excelImportService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _excelImportService = excelImportService;
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

            return View(transactions);
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

            using var stream = file.OpenReadStream();
            await _excelImportService.ImportTransactionsAsync(stream, userId, defaultCategoryId: 1, defaultAccountId: 1);

            TempData["Success"] = "Transactions imported successfully!";
            return RedirectToAction("Index");
        }
    }
}
