using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinScope.Models
{
    public class Asset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The name must be between 1 and 100 characters long.", MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Asset Type should be within 1 to 100 characters.")]
        public string AssetType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "The value must be greater than 0.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "The value must have a maximum of two decimal places.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }


        public DateTime? DateAcquired { get; set; }
    }
}
