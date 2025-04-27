using finalProject.Abstractions;
using finalProject.Data;
using finalProject.Models;
using finalProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace finalProject.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly SpreadsheetImportService _spreadsheetImportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionService(ApplicationDbContext context, SpreadsheetImportService spreadsheetImportService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _spreadsheetImportService = spreadsheetImportService;
            _userManager = userManager;
        }

        public async Task<TransactionIndexViewModel> GetAllTransactions(string userId, int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.TransactionDateTime) 
                .Skip(skip)
                .Take(pageSize)
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

            var totalCount = await _context.Transactions
                .Where(t => t.UserId == userId)
                .CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var categories = await _context.Categories.ToListAsync();
            var accounts = await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();

            return new TransactionIndexViewModel
            {
                Transactions = transactionVMs,
                TotalBalance = totalBalance,
                Categories = categories,
                Accounts = accounts,
                PageNumber = pageNumber,    
                PageSize = pageSize,        
                TotalCount = totalCount,    
                TotalPages = totalPages    
            };
        }


        public async Task<bool> ManualAddTransaction(TransactionIndexViewModel model, string userId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == model.SelectedAccountId && a.UserId == userId);
            if (account == null) return false;

            Category category = null;
            if (!string.IsNullOrWhiteSpace(model.ManualCategoryName))
            {
                category = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == model.ManualCategoryName.Trim().ToLower());
                if (category == null)
                {
                    category = new Category { Name = model.ManualCategoryName.Trim() };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == model.SelectedCategoryId);
            }

            if (category == null) return false;

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

            return true;
        }

        public async Task<bool> ImportTransactions(IFormFile file, string userId)
        {
            if (file == null || file.Length == 0) return false;

            var defaultAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId)
                ?? new Account { Name = "Default Account", Balance = 0, UserId = userId };

            if (defaultAccount.Id == 0)
            {
                _context.Accounts.Add(defaultAccount);
                await _context.SaveChangesAsync();
            }

            var defaultCategory = await _context.Categories.FirstOrDefaultAsync()
                ?? new Category { Name = "Miscellaneous" };

            if (defaultCategory.Id == 0)
            {
                _context.Categories.Add(defaultCategory);
                await _context.SaveChangesAsync();
            }

            using var stream = file.OpenReadStream();
            await _spreadsheetImportService.ImportTransactionsAsync(stream, userId, defaultCategory.Id, defaultAccount.Id);

            return true;
        }

        public async Task<bool> MoveTransaction(int transactionId, int newAccountId, string userId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

            var newAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == newAccountId && a.UserId == userId);

            if (transaction == null || newAccount == null || transaction.AccountId == newAccountId) return false;

            transaction.Account.Balance -= transaction.Amount;
            newAccount.Balance += transaction.Amount;
            transaction.AccountId = newAccountId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTransaction(int id, string userId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null) return false;

            if (transaction.Account != null)
            {
                transaction.Account.Balance -= transaction.Amount;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TransferTransaction(int transactionId, int targetAccountId, decimal transferAmount, string userId)
        {
            var originalTransaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);

            var targetAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == targetAccountId && a.UserId == userId);

            if (originalTransaction == null || targetAccount == null || transferAmount <= 0) return false;

            if (Math.Abs(transferAmount) > Math.Abs(originalTransaction.Amount)) return false;

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

            if (originalTransaction.Amount < 0)
            {
                originalTransaction.Account.Balance -= transferAmount;
                targetAccount.Balance -= transferAmount;
                originalTransaction.Amount += transferAmount;
            }
            else
            {
                originalTransaction.Account.Balance -= transferAmount;
                targetAccount.Balance += transferAmount;
                originalTransaction.Amount -= transferAmount;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<TransactionIndexViewModel> GetPaginatedTransactions(string userId, int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.TransactionDateTime)
                .Skip(skip)
                .Take(pageSize)
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

            var categories = await _context.Categories.ToListAsync();
            var accounts = await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();

            return new TransactionIndexViewModel
            {
                Transactions = transactionVMs,
                Categories = categories,
                Accounts = accounts,
                TotalBalance = await _context.Accounts.Where(a => a.UserId == userId).SumAsync(a => a.Balance),
                PageSize = pageSize,
                PageNumber = pageNumber,
                TotalCount = await _context.Transactions.Where(t => t.UserId == userId).CountAsync(),
                TotalPages = (int)Math.Ceiling((double)await _context.Transactions.Where(t => t.UserId == userId).CountAsync() / pageSize)
            };
        }

        public async Task<int> GetTransactionCount(string userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .CountAsync();
        }

    }
}
