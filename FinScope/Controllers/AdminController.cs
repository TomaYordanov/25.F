using FinScope.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinScope.ViewModels;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Index()
    {
        return View(); 
    }
    public async Task<IActionResult> Users(int page = 1)
    {
        var users = _userManager.Users;
        var pageSize = 10;
        var totalUsers = await users.CountAsync();
        var usersOnPage = await users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new UsersViewModel
        {
            Users = usersOnPage,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize)
        };

        return View(model);
    }


    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(ApplicationUser model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        user.Email = model.Email;
        user.UserName = model.Email;
        user.Name = model.Name;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Users));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Index");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("admin", StringComparer.OrdinalIgnoreCase))
        {
            TempData["Error"] = "You cannot delete an admin user.";
            return RedirectToAction("Index");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "User deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Error deleting user.";
        }

        return RedirectToAction("Index");
    }

}
