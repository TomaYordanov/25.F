using finalProject.Models;
using System.ComponentModel.DataAnnotations;

namespace finalProject.ViewModels
{
    public class TransactionIndexViewModel
    {
        public List<TransactionViewModel> Transactions { get; set; } = new List<TransactionViewModel>();
        public decimal TotalBalance { get; set; }

        public List<Category> Categories { get; set; } = new();
        public List<Account> Accounts { get; set; } = new();


        [Display(Name = "Date & Time")]
        public DateTime ManualDateTime { get; set; } = DateTime.Now;

        [Required]
        public decimal ManualAmount { get; set; }

        public string ManualDescription { get; set; }

        [Display(Name = "Category")]
        public int SelectedCategoryId { get; set; }

        [Display(Name = "Account")]
        public int SelectedAccountId { get; set; }
    }
}
