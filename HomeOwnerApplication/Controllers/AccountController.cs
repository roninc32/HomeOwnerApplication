using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using HomeOwnerApplication.Models;
using HomeOwnerApplication.ViewModels;
using HomeOwnerApplication.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace HomeOwnerApplication.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserActivityTracker _activityTracker;
        private readonly DateTime _currentTime = DateTime.Parse("2025-03-20 21:52:23");
        private const string CurrentUser = "roninc32";

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IWebHostEnvironment environment,
            IUserActivityTracker activityTracker)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _activityTracker = activityTracker ?? throw new ArgumentNullException(nameof(activityTracker));
        }

        #region Login/Logout Actions

        [HttpGet("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                LoginAttemptTime = _currentTime
            };
            return View(model);
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.LoginAttemptTime = _currentTime;
            model.ReturnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        if (!await _userManager.IsEmailConfirmedAsync(user))
                        {
                            ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                            _logger.LogWarning("Unconfirmed email login attempt for {Email} at {Time} by {User}",
                                model.Email, _currentTime, CurrentUser);
                            return View(model);
                        }

                        var result = await _signInManager.PasswordSignInAsync(
                            user.UserName ?? string.Empty,
                            model.Password,
                            model.RememberMe,
                            lockoutOnFailure: true);

                        if (result.Succeeded)
                        {
                            user.LastLoginAt = _currentTime;
                            user.ModifiedBy = CurrentUser;
                            await _userManager.UpdateAsync(user);

                            await _activityTracker.LogActivityAsync(user.Id, "Login", _currentTime);
                            _logger.LogInformation("User {Email} logged in successfully at {Time} by {User}",
                                model.Email, _currentTime, CurrentUser);

                            return LocalRedirect(model.ReturnUrl);
                        }
                        if (result.RequiresTwoFactor)
                        {
                            return RedirectToAction(nameof(LoginWith2fa), new { returnUrl = model.ReturnUrl, rememberMe = model.RememberMe });
                        }
                        if (result.IsLockedOut)
                        {
                            _logger.LogWarning("User {Email} account locked out at {Time} by {User}",
                                model.Email, _currentTime, CurrentUser);
                            return RedirectToAction(nameof(Lockout));
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    _logger.LogWarning("Failed login attempt for {Email} at {Time} by {User}",
                        model.Email, _currentTime, CurrentUser);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login attempt for user {Email} at {Time} by {User}",
                        model.Email, _currentTime, CurrentUser);
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                }
            }

            return View(model);
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email ?? "Unknown";

            try
            {
                if (user != null)
                {
                    await _activityTracker.LogActivityAsync(user.Id, "Logout", _currentTime);
                }

                await _signInManager.SignOutAsync();
                _logger.LogInformation("User {Email} logged out at {Time} by {User}",
                    email, _currentTime, CurrentUser);

                if (returnUrl != null)
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {Email} at {Time} by {User}",
                    email, _currentTime, CurrentUser);
                throw;
            }
        }

        #endregion

        #region Registration Actions

        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new RegisterViewModel
            {
                RegisteredAt = _currentTime
            };
            return View(model);
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _userManager.FindByEmailAsync(model.Email) != null)
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                        _logger.LogWarning("Registration attempt with existing email {Email} at {Time} by {User}",
                            model.Email, _currentTime, CurrentUser);
                        return View(model);
                    }

                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        PropertyAddress = model.PropertyAddress,
                        EmailConfirmed = false,
                        CreatedAt = _currentTime,
                        LastLoginAt = null,
                        LockoutEnabled = true,
                        CreatedBy = CurrentUser,
                        ModifiedBy = CurrentUser,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("New user {Email} created at {Time} by {User}",
                            model.Email, _currentTime, CurrentUser);

                        var roleResult = await _userManager.AddToRoleAsync(user, "HomeOwner");
                        if (!roleResult.Succeeded)
                        {
                            _logger.LogError("Failed to add role for {Email} at {Time} by {User}",
                                model.Email, _currentTime, CurrentUser);
                            await _userManager.DeleteAsync(user);
                            ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                            return View(model);
                        }

                        await _activityTracker.LogActivityAsync(user.Id, "Registration", _currentTime);

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action(
                            "ConfirmEmail",
                            "Account",
                            new { userId = user.Id, code = code },
                            protocol: Request.Scheme);

                        if (_environment.IsDevelopment())
                        {
                            await _userManager.ConfirmEmailAsync(user, code);
                            await _activityTracker.LogActivityAsync(user.Id, "Email Auto-Confirmed", _currentTime);
                        }

                        return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email });
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email} at {Time} by {User}",
                    model.Email, _currentTime, CurrentUser);
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
            }

            return View(model);
        }

        [HttpGet("register-confirmation")]
        public IActionResult RegisterConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["Email"] = email;
            return View();
        }

        #endregion

        #region Email Confirmation

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Invalid user ID {UserId} for email confirmation at {Time} by {User}",
                        userId, _currentTime, CurrentUser);
                    return NotFound("Unable to load user.");
                }

                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to confirm email for user {Email} at {Time} by {User}",
                        user.Email, _currentTime, CurrentUser);
                    return View("Error");
                }

                await _activityTracker.LogActivityAsync(userId, "Email Confirmed", _currentTime);
                _logger.LogInformation("Email confirmed for user {Email} at {Time} by {User}",
                    user.Email, _currentTime, CurrentUser);
                return View("ConfirmEmail");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user ID {UserId} at {Time} by {User}",
                    userId, _currentTime, CurrentUser);
                return View("Error");
            }
        }

        #endregion

        #region Password Reset

        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                    {
                        _logger.LogWarning("Password reset attempted for invalid email {Email} at {Time} by {User}",
                            model.Email, _currentTime, CurrentUser);
                        return RedirectToAction(nameof(ForgotPasswordConfirmation));
                    }

                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ResetPassword",
                        "Account",
                        new { code },
                        protocol: Request.Scheme);

                    await _activityTracker.LogActivityAsync(user.Id, "Password Reset Requested", _currentTime);
                    _logger.LogInformation("Password reset token generated for {Email} at {Time} by {User}",
                        model.Email, _currentTime, CurrentUser);
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in password reset for {Email} at {Time} by {User}",
                        model.Email, _currentTime, CurrentUser);
                    ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                }
            }

            return View(model);
        }

        [HttpGet("forgot-password-confirmation")]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region Error Pages

        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet("lockout")]
        public IActionResult Lockout()
        {
            return View();
        }

        #endregion
    }
}