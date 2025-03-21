using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HomeOwnerApplication.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            UserRoles = new List<IdentityUserRole<string>>();
            CreatedAt = DateTime.Parse("2025-03-20 22:17:00");
            ModifiedAt = DateTime.Parse("2025-03-20 22:17:00");
            CreatedBy = "roninc32";
            ModifiedBy = "roninc32";
        }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string PropertyAddress { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string? EmergencyContactName { get; set; }

        [Phone]
        public string? EmergencyContactPhone { get; set; }

        [Required]
        [StringLength(256)]
        public string CreatedBy { get; set; }

        [Required]
        [StringLength(256)]
        public string ModifiedBy { get; set; }

        [Required]
        public DateTime ModifiedAt { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; }
    }
}