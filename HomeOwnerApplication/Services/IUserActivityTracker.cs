namespace HomeOwnerApplication.Services
{
    public interface IUserActivityTracker
    {
        Task LogActivityAsync(string userId, string activity, DateTime timestamp);
    }
}