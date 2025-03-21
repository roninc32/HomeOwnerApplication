namespace HomeOwnerApplication.ViewModels
{
    public class UserViewModel
    {
        public required string Id { get; set; } = string.Empty;
        public required string UserName { get; set; } = string.Empty;
        public required string Email { get; set; } = string.Empty;
        public required string FirstName { get; set; } = string.Empty;
        public required string LastName { get; set; } = string.Empty;
        public required List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.Parse("2025-03-20 20:38:06");
        public DateTime LastModified { get; set; } = DateTime.Parse("2025-03-20 20:38:06");
    }
}