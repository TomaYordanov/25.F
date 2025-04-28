using finalProject.Abstractions;
using finalProject.Data;
using finalProject.Models;
using finalProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace finalProject.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId)
        {
            try
            {
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

                return new DashboardViewModel
                {
                    Accounts = accounts,
                    TotalBalance = accounts.Sum(a => a.Balance)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching dashboard data.", ex);
            }
        }

        public async Task<bool> AddAccountAsync(string userId, DashboardViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.NewAccountName))
                    return false;

                var newAccount = new Account
                {
                    Name = model.NewAccountName.Trim(),
                    Balance = model.NewAccountBalance,
                    UserId = userId
                };

                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the account.", ex);
            }
        }

        public async Task<bool> DeleteAccountAsync(string userId, int accountId)
        {
            try
            {
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

                if (account == null || account.Name == "Main")
                    return false;

                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the account.", ex);
            }
        }
    }
}
