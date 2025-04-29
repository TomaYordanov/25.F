using finalProject.Data;
using finalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace finalProject.Services
{
    public class UserSeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserSeedService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedUsers()
        {
            await _context.Database.MigrateAsync();

            // Ensure the Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Admin User 
            var adminUser = await _userManager.FindByEmailAsync("admin@default.com");

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@default.com",
                    Email = "admin@default.com",
                    Name = "Admin User",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, "Password123!");

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create admin user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Regular User 1
            var user1 = await _userManager.FindByEmailAsync("user1@default.com");

            if (user1 == null)
            {
                user1 = new ApplicationUser
                {
                    UserName = "user1@default.com",
                    Email = "user1@default.com",
                    Name = "User One",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user1, "Password123!");

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create user1: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Regular User 2 
            var user2 = await _userManager.FindByEmailAsync("user2@default.com");

            if (user2 == null)
            {
                user2 = new ApplicationUser
                {
                    UserName = "user2@default.com",
                    Email = "user2@default.com",
                    Name = "User Two",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user2, "Password123!");

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create user2: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

    }
}
