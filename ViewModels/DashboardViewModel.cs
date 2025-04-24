using System.ComponentModel.DataAnnotations;

namespace finalProject.ViewModels
{
    public class DashboardViewModel
    {
        [Required]
        [Display(Name = "Account Name")]
        public string NewAccountName { get; set; }

        [Display(Name = "Initial Balance")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance must be non-negative.")]
        public decimal NewAccountBalance { get; set; }
    }
}
