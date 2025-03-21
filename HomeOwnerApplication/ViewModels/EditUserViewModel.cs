using System.ComponentModel.DataAnnotations;

namespace HomeOwnerApplication.ViewModels
{
    public class EditUserViewModel : BaseViewModel
    {
        public required string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public required string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public required string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public required string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public required string Role { get; set; } = string.Empty;
    }
}