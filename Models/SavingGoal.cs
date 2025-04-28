using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace finalProject.Models
{
    public class SavingGoal
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Display(Name = "Target Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }

        [Display(Name = "Current Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAmount { get; set; }

        public DateTime? Deadline { get; set; }

        public bool IsCompleted { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }  
    }
}
