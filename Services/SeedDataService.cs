using finalProject.Data;
using finalProject.Models;
using Microsoft.AspNetCore.Identity;

namespace finalProject.Services
{
    public class SeedDataService
    {
        private readonly ApplicationDbContext _context;

        public SeedDataService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!_context.Categories.Any())
            {
                _context.Categories.AddRange(
                    new Category { Name = "Food" },
                    new Category { Name = "Transport" },
                    new Category { Name = "Utilities" },
                    new Category { Name = "Entertainment" }
                );
            }

            if (!_context.Accounts.Any())
            {
                _context.Accounts.Add(new Account
                {
                    Name = "Main Bank Account",
                    Balance = 1000
                });

            }

            if (!_context.Users.Any())
            {
                var user = new ApplicationUser
                {
                    UserName = "demo@finance.com",
                    Email = "demo@finance.com",
                    Name = "Demo User"
                };
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var account = _context.Accounts.First();
                var category = _context.Categories.First();

                _context.Transactions.AddRange(
                    new Transaction
                    {
                        Amount = 50,
                        Date = DateTime.Now.AddDays(-1),
                        Description = "Groceries",
                        AccountId = account.Id,
                        CategoryId = category.Id,
                        UserId = user.Id
                    },
                    new Transaction
                    {
                        Amount = 25,
                        Date = DateTime.Now,
                        Description = "Taxi",
                        AccountId = account.Id,
                        CategoryId = category.Id,
                        UserId = user.Id
                    }
                );
            }

            await _context.SaveChangesAsync();
        }
    }

}
