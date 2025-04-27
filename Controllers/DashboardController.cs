using finalProject.Abstractions;
using finalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using finalProject.Models;

namespace finalProject.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(IDashboardService dashboardService, UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var model = await _dashboardService.GetDashboardDataAsync(userId);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred while fetching the dashboard data.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAccount(DashboardViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Invalid account details.";
                    return RedirectToAction("Index");
                }

                var result = await _dashboardService.AddAccountAsync(userId, model);

                if (!result)
                {
                    TempData["Error"] = "Failed to add account.";
                    return RedirectToAction("Index");
                }

                TempData["Success"] = "Account created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding the account.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _dashboardService.DeleteAccountAsync(userId, accountId);

                if (!result)
                {
                    TempData["Error"] = "Failed to delete account.";
                }
                else
                {
                    TempData["Success"] = "Account deleted successfully!";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the account.";
                return RedirectToAction("Index");
            }
        }
    }
}
