using finalProject.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace finalProject.Abstractions
{
    public interface ITransactionService
    {
        Task<TransactionIndexViewModel> GetAllTransactions(string userId, int pageNumber, int pageSize);
        Task<int> GetTransactionCount(string userId);
        Task<TransactionIndexViewModel> GetPaginatedTransactions(string userId, int pageNumber, int pageSize);
        Task<bool> ManualAddTransaction(TransactionIndexViewModel model, string userId);
        Task<bool> ImportTransactions(IFormFile file, string userId);
        Task<bool> MoveTransaction(int transactionId, int newAccountId, string userId);
        Task<bool> DeleteTransaction(int id, string userId);
        Task<bool> TransferTransaction(int transactionId, int targetAccountId, decimal transferAmount, string userId);
    }
}
