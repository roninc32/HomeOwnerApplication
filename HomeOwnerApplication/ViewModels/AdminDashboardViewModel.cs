namespace HomeOwnerApplication.ViewModels
{
    public class AdminDashboardViewModel
    {
        public DateTime LastAccessTime { get; set; } = DateTime.Parse("2025-03-20 20:38:06");
        public required string AdminName { get; set; } = "roninc32";
        public int TotalUsers { get; set; }
        public int TotalHomeOwners { get; set; }
        public int TotalStaff { get; set; }
        public required List<string> RecentActivities { get; set; } = new List<string>();
        public DateTime LastModified { get; set; } = DateTime.Parse("2025-03-20 20:38:06");
    }
}