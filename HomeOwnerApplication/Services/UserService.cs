using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HomeOwnerApplication.Models;
using HomeOwnerApplication.Data;

namespace HomeOwnerApplication.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly DateTime _currentTime = DateTime.Parse("2025-03-20 22:07:56");
        private const string CurrentUser = "roninc32";
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<UserService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            try
            {
                return await _userManager.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users at {Time} by {User}", _currentTime, CurrentUser);
                throw;
            }
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            try
            {
                return await _userManager.FindByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} at {Time} by {User}", id, _currentTime, CurrentUser);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            try
            {
                var existingUser = await _userManager.FindByIdAsync(user.Id);
                if (existingUser == null)
                {
                    _logger.LogWarning("User {UserId} not found for update at {Time} by {User}",
                        user.Id, _currentTime, CurrentUser);
                    return false;
                }

                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.PropertyAddress = user.PropertyAddress;
                existingUser.EmergencyContactName = user.EmergencyContactName;
                existingUser.EmergencyContactPhone = user.EmergencyContactPhone;
                existingUser.ModifiedBy = CurrentUser;

                var result = await _userManager.UpdateAsync(existingUser);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} updated at {Time} by {User}",
                        user.Id, _currentTime, CurrentUser);
                }
                else
                {
                    _logger.LogWarning("Failed to update user {UserId} at {Time} by {User}. Errors: {Errors}",
                        user.Id, _currentTime, CurrentUser, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId} at {Time} by {User}",
                    user.Id, _currentTime, CurrentUser);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for deletion at {Time} by {User}",
                        id, _currentTime, CurrentUser);
                    return false;
                }

                // Soft delete - just mark as inactive
                user.IsActive = false;
                user.ModifiedBy = CurrentUser;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} marked as inactive at {Time} by {User}",
                        id, _currentTime, CurrentUser);
                }
                else
                {
                    _logger.LogWarning("Failed to mark user {UserId} as inactive at {Time} by {User}. Errors: {Errors}",
                        id, _currentTime, CurrentUser, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId} at {Time} by {User}",
                    id, _currentTime, CurrentUser);
                throw;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for role change at {Time} by {User}",
                        userId, _currentTime, CurrentUser);
                    return false;
                }

                if (!await _roleManager.RoleExistsAsync(newRole))
                {
                    _logger.LogWarning("Role {Role} does not exist at {Time}. Attempted by {User}",
                        newRole, _currentTime, CurrentUser);
                    return false;
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                var result = await _userManager.AddToRoleAsync(user, newRole);
                if (result.Succeeded)
                {
                    user.ModifiedBy = CurrentUser;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("Role changed to {Role} for user {UserId} at {Time} by {User}",
                        newRole, userId, _currentTime, CurrentUser);
                }
                else
                {
                    _logger.LogWarning("Failed to change role to {Role} for user {UserId} at {Time} by {User}. Errors: {Errors}",
                        newRole, userId, _currentTime, CurrentUser, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing role for user {UserId} at {Time} by {User}",
                    userId, _currentTime, CurrentUser);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(ApplicationUser user)
        {
            try
            {
                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user {UserId} at {Time} by {User}",
                    user.Id, _currentTime, CurrentUser);
                throw;
            }
        }
    }
}