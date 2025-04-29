using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace finalProject.Models
{
    public class SavingGoal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title must be 100 characters or less.")]
        public string Title { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Target Amount must be greater than 0.")]
        [Display(Name = "Target Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }


        [Required(ErrorMessage = "Current Amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Current Amount cannot be negative.")]
        [Display(Name = "Current Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAmount { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Deadline")]
        public DateTime? Deadline { get; set; }

        public bool IsCompleted { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
