using finalProject.Data;
using finalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class AssetsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssetsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<string> GetUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id;
    }

    public async Task<IActionResult> Index()
    {
        var userId = await GetUserIdAsync();
        var assets = await _context.Assets.Where(a => a.UserId == userId).ToListAsync();
        return View(assets);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Asset asset)
    {
        var userId = await GetUserIdAsync();
        if (ModelState.ErrorCount <= 3)
        {
            asset.UserId = userId;
            _context.Add(asset);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Asset added successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(asset); 
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null)
        {
            return NotFound();
        }
        return View(asset);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Asset asset)
    {
        if (ModelState.ErrorCount <= 2)
        {
            _context.Update(asset);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Asset updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(asset); 
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset != null)
        {
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Asset deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }
}
