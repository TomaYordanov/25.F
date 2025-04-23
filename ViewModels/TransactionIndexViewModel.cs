using System.Collections.Generic;

namespace finalProject.ViewModels
{
    public class TransactionIndexViewModel
    {
        public List<TransactionViewModel> Transactions { get; set; } = new List<TransactionViewModel>();
        public decimal TotalBalance { get; set; }
    }
}
