using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeOwnerApplication.Models;
using HomeOwnerApplication.Services;
using HomeOwnerApplication.ViewModels;

namespace HomeOwnerApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserService _userService;
        private readonly DateTime _currentTime = DateTime.Parse("2025-03-20 20:55:50");
        private const string AdminUsername = "roninc32";

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserService userService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            try
            {
                var model = new CreateUserViewModel
                {
                    Email = string.Empty,
                    Password = string.Empty,
                    ConfirmPassword = string.Empty,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Role = string.Empty,
                    CreatedAt = _currentTime
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error initializing create user form: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        return View(model);
                    }

                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        ModelState.AddModelError("Role", "Selected role is invalid");
                        return View(model);
                    }

                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        EmailConfirmed = true,
                        CreatedAt = _currentTime,
                        LockoutEnabled = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                        if (roleResult.Succeeded)
                        {
                            TempData["SuccessMessage"] = $"User {model.Email} created successfully with role {model.Role}";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            await _userManager.DeleteAsync(user);
                            ModelState.AddModelError("", "Failed to assign role. User creation rolled back.");
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while creating the user: {ex.Message}");
                return View(model);
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var totalUsers = await _userManager.Users.CountAsync();
                var homeOwners = await _userManager.GetUsersInRoleAsync("HomeOwner");
                var staff = await _userManager.GetUsersInRoleAsync("Staff");

                var model = new AdminDashboardViewModel
                {
                    LastAccessTime = _currentTime,
                    AdminName = AdminUsername,
                    TotalUsers = totalUsers,
                    TotalHomeOwners = homeOwners.Count,
                    TotalStaff = staff.Count,
                    RecentActivities = new List<string>
                    {
                        $"Last login: {_currentTime:yyyy-MM-dd HH:mm:ss UTC}",
                        $"Total users in system: {totalUsers}",
                        $"Home Owners: {homeOwners.Count}",
                        $"Staff Members: {staff.Count}"
                    }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var userViewModels = new List<UserViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userService.GetUserRolesAsync(user);
                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id ?? string.Empty,
                        UserName = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty,
                        Roles = roles?.ToList() ?? new List<string>(),
                        CreatedAt = user.CreatedAt,
                        LastModified = _currentTime
                    });
                }

                return View(userViewModels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading users: {ex.Message}";
                return View(new List<UserViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("User ID is required");
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                var roles = await _userService.GetUserRolesAsync(user);
                var viewModel = new EditUserViewModel
                {
                    Id = user.Id ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    CreatedAt = _currentTime
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading user for edit: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userService.GetUserByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound($"User with ID {model.Id} not found");
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;

                var result = await _userService.UpdateUserAsync(user);
                if (result)
                {
                    var currentRoles = await _userService.GetUserRolesAsync(user);
                    if (!string.IsNullOrEmpty(model.Role) && !currentRoles.Contains(model.Role))
                    {
                        await _userService.ChangeUserRoleAsync(user.Id, model.Role);
                    }

                    TempData["SuccessMessage"] = "User updated successfully";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Failed to update user");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating the user: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("User ID is required");
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                if (user.UserName == AdminUsername)
                {
                    TempData["ErrorMessage"] = "Cannot delete the admin account";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading user for deletion: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("User ID is required");
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user?.UserName == AdminUsername)
                {
                    TempData["ErrorMessage"] = "Cannot delete the admin account";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userService.DeleteUserAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "User deleted successfully";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "Failed to delete user";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult UserManagement()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}