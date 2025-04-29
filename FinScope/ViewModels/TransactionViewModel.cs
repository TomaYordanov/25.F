namespace FinScope.ViewModels
{
    public class TransactionViewModel
    {
        public int Id { get; set; } 
        public DateTime TransactionDateTime { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string AccountName { get; set; }
    }

}
