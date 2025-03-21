using Microsoft.AspNetCore.Mvc;
using HomeOwnerApplication.Models;

namespace HomeOwnerApplication.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DateTime _currentTime = DateTime.Parse("2025-03-21 04:13:03");
        private const string CurrentUser = "roninc32";

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.CurrentTime = _currentTime;
            ViewBag.CurrentUser = CurrentUser;
            return View();
        }
    }
}