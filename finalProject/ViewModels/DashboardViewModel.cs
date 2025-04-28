using finalProject.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace finalProject.ViewModels
{
    public class DashboardViewModel
    {
        public List<Account> Accounts { get; set; } = new();

        [Required]
        [Display(Name = "Account Name")]
        public string NewAccountName { get; set; }

        [Display(Name = "Initial Balance")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance must be non-negative.")]
        public decimal NewAccountBalance { get; set; }
        public decimal NetWorth { get; set; }
        public decimal TotalBalance { get; set; }

        public int TransactionCount { get; set; }

        public int CategoryCount { get; set; }

        public decimal TotalSpent { get; set; }

        public List<Asset> Assets { get; set; }
        public decimal TotalAssetValue { get; set; }
    }
}
