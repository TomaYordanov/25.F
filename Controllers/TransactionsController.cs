using finalProject.Abstractions;
using finalProject.Models;
using finalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class TransactionsController : Controller
{
    private readonly ITransactionService _transactionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TransactionsController(ITransactionService transactionService, UserManager<ApplicationUser> userManager)
    {
        _transactionService = transactionService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var model = await _transactionService.GetAllTransactions(userId);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ManualAdd(TransactionIndexViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (!await _transactionService.ManualAddTransaction(model, userId))
        {
            TempData["Error"] = "Failed to add transaction.";
            return RedirectToAction("Index");
        }

        TempData["Success"] = "Transaction added successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Import(IFormFile file)
    {
        var userId = _userManager.GetUserId(User);
        if (!await _transactionService.ImportTransactions(file, userId))
        {
            TempData["Error"] = "Failed to import transactions.";
            return RedirectToAction("Index");
        }

        TempData["Success"] = "Transactions imported successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> MoveTransaction(int transactionId, int newAccountId)
    {
        var userId = _userManager.GetUserId(User);
        if (!await _transactionService.MoveTransaction(transactionId, newAccountId, userId))
        {
            TempData["Error"] = "Failed to move transaction.";
            return RedirectToAction("Index");
        }

        TempData["Success"] = "Transaction moved successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (!await _transactionService.DeleteTransaction(id, userId))
        {
            TempData["Error"] = "Failed to delete transaction.";
            return RedirectToAction("Index");
        }

        TempData["Success"] = "Transaction deleted successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> TransferTransaction(int transactionId, int targetAccountId, decimal transferAmount)
    {
        var userId = _userManager.GetUserId(User);
        if (!await _transactionService.TransferTransaction(transactionId, targetAccountId, transferAmount, userId))
        {
            TempData["Error"] = "Failed to transfer transaction.";
            return RedirectToAction("Index");
        }

        TempData["Success"] = "Transfer completed successfully!";
        return RedirectToAction("Index");
    }
}
