using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HomeOwnerApplication.Models;

namespace HomeOwnerApplication.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DateTime _currentTime = DateTime.Parse("2025-03-21 04:41:08");
        private const string CurrentUser = "roninc32";

        public DashboardController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            ViewBag.CurrentTime = _currentTime;
            ViewBag.CurrentUser = CurrentUser;
            ViewBag.UserRole = role;
            ViewBag.UserName = $"{user.FirstName} {user.LastName}";

            switch (role?.ToLower())
            {
                case "admin":
                    return View("AdminDashboard");
                case "staff":
                    return View("StaffDashboard");
                case "homeowner":
                    return View("HomeOwnerDashboard");
                default:
                    return RedirectToAction("AccessDenied", "Account");
            }
        }
    }
}