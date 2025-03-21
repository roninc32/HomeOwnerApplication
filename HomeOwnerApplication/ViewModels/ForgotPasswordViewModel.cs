using System.ComponentModel.DataAnnotations;

namespace HomeOwnerApplication.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public DateTime RequestTime { get; set; } = DateTime.Parse("2025-03-20 21:28:22");
    }
}