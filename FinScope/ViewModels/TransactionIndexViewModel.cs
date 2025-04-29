using FinScope.Models;
using System.ComponentModel.DataAnnotations;

namespace FinScope.ViewModels
{
    public class TransactionIndexViewModel
    {
        public List<TransactionViewModel> Transactions { get; set; }
        public decimal TotalBalance { get; set; }
        public List<Category> Categories { get; set; }
        public List<Account> Accounts { get; set; }

        // Manual transaction input fields
        [Display(Name = "Date & Time")]
        public DateTime ManualDateTime { get; set; }

        [Display(Name = "Amount")]
        public decimal ManualAmount { get; set; }

        [Display(Name = "Description")]
        public string ManualDescription { get; set; }

        [Display(Name = "Choose Existing Category")]
        public int SelectedCategoryId { get; set; }

        [Display(Name = "Account")]
        public int SelectedAccountId { get; set; }

        [Display(Name = "Or Create New Category")]
        public string ManualCategoryName { get; set; }

        // Pagination properties
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
    }

}
