namespace HomeOwnerApplication.ViewModels
{
    public abstract class BaseViewModel
    {
        public DateTime CreatedAt { get; set; } = DateTime.Parse("2025-03-20 20:31:41");
        public string CurrentUser { get; set; } = "roninc32";
    }
}