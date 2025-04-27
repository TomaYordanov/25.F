using System.ComponentModel.DataAnnotations.Schema;

namespace finalProject.Models
{
    public class SavingGoal
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        public DateTime? Deadline { get; set; }

        public bool IsCompleted { get; set; }

        public string Notes { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        

    }
}
