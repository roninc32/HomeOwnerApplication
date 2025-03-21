using HomeOwnerApplication.Data;
using HomeOwnerApplication.Models;
using Microsoft.Extensions.Logging;

namespace HomeOwnerApplication.Services
{
    public class UserActivityTracker : IUserActivityTracker
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserActivityTracker> _logger;
        private readonly DateTime _currentTime = DateTime.Parse("2025-03-20 22:10:26");
        private const string CurrentUser = "roninc32";

        public UserActivityTracker(
            ApplicationDbContext context,
            ILogger<UserActivityTracker> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task LogActivityAsync(string userId, string activity, DateTime timestamp)
        {
            try
            {
                var userActivity = new UserActivity
                {
                    UserId = userId,
                    ActivityType = activity,
                    ActivityTime = timestamp,
                    CreatedBy = CurrentUser,
                    CreatedAt = _currentTime,
                    ModifiedBy = CurrentUser,
                    ModifiedAt = _currentTime
                };

                await _context.UserActivities.AddAsync(userActivity);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Activity logged: {Activity} for user {UserId} at {Time} by {User}",
                    activity, userId, timestamp, CurrentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to log activity {Activity} for user {UserId} at {Time} by {User}",
                    activity, userId, timestamp, CurrentUser);
                throw;
            }
        }
    }
}