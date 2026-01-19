using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Models.Users;
using MovieRental.Services;
using MovieRental.ViewModels.Account;

namespace MovieRental.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly ILoggerService _loggerService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger,
        ILoggerService loggerService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _loggerService = loggerService;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created a new account", model.Email);
                _loggerService.LogUserAction(user.Id, "Register", $"New user registered: {model.Email}");
                _loggerService.LogSecurityEvent("UserRegistration", $"New account created for {model.Email}", user.Id);

                await _userManager.AddToRoleAsync(user, "Customer");

                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] = "Welcome! Your account has been created successfully.";

                return RedirectToLocal(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                _logger.LogWarning("Registration failed for {Email}: {Error}", model.Email, error.Description);
            }
        }

        return View(model);
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully", model.Email);
                
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    _loggerService.LogUserAction(user.Id, "Login", $"User logged in: {model.Email}");
                    _loggerService.LogSecurityEvent("LoginSuccess", $"Successful login for {model.Email}", user.Id);
                    
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account locked out", model.Email);
                _loggerService.LogSecurityEvent("AccountLockout", $"Account locked out: {model.Email}");
                return View("Lockout");
            }

            _logger.LogWarning("Invalid login attempt for {Email}", model.Email);
            _loggerService.LogSecurityEvent("LoginFailed", $"Failed login attempt for {model.Email}");
            
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);
        var email = User.Identity?.Name;

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User {Email} logged out", email);
        
        if (!string.IsNullOrEmpty(userId))
        {
            _loggerService.LogUserAction(userId, "Logout", $"User logged out: {email}");
        }

        TempData["SuccessMessage"] = "You have been logged out successfully.";
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        var userId = _userManager.GetUserId(User);
        var path = HttpContext.Request.Path;
        
        _logger.LogWarning("Access denied for user {UserId} attempting to access {Path}", userId, path);
        _loggerService.LogSecurityEvent("AccessDenied", $"Unauthorized access attempt to {path}", userId);

        return View();
    }

    // GET: /Account/Lockout
    [HttpGet]
    public IActionResult Lockout()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}