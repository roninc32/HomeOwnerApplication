using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeOwnerApplication.Models
{
    [Table("UserActivities", Schema = "identity")]
    public class UserActivity
    {
        public UserActivity()
        {
            CreatedAt = DateTime.Parse("2025-03-20 22:17:00");
            ModifiedAt = DateTime.Parse("2025-03-20 22:17:00");
            CreatedBy = "roninc32";
            ModifiedBy = "roninc32";
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; } = string.Empty;

        [Required]
        public DateTime ActivityTime { get; set; }

        [Required]
        [StringLength(256)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(256)]
        public string ModifiedBy { get; set; }

        [Required]
        public DateTime ModifiedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}