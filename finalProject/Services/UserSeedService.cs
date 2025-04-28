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

        public async Task SeedDefaultUserAsync()
        {
            await _context.Database.MigrateAsync();

            if (!_context.Users.Any())
            {
                var defaultUser = new ApplicationUser
                {
                    UserName = "admin@default.com",
                    Email = "admin@default.com",
                    Name = "Admin User", 
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(defaultUser, "Password123!");

                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create default user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                await _userManager.AddToRoleAsync(defaultUser, "Admin");
            }
        }
    }
}
