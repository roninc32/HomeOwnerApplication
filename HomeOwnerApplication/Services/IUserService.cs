using HomeOwnerApplication.Models;

namespace HomeOwnerApplication.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> ChangeUserRoleAsync(string userId, string newRole);
        Task<IEnumerable<string>> GetUserRolesAsync(ApplicationUser user);
    }
}