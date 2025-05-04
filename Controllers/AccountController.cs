using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SciArticle.Models;
using SciArticle.Models.Front.User;
using SciArticle.Models.Object;
using SciArticle.Models.Utilities;
using SciArticle.Models.Back;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SciArticle.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger, MainContext context)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null!)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null!)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = UserQuery.GetUserByUsername(model.Username);
        if (user == null || !PasswordHasher.VerifyPassword(model.Password, user.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, user.Username),
            new (ClaimTypes.Email, user.Email),
            new (ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            RedirectUri = returnUrl
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        _logger.LogInformation("User {Username} logged in at {Time}", user.Username, DateTime.UtcNow);

        // Check if the user is an author and redirect to the author dashboard
        if (user.Role == UserRole.Author)
        {
            return RedirectToAction("Dashboard", "Author");
        }

        // Otherwise use the returnUrl or default to the home page
        return LocalRedirect(returnUrl ?? Url.Content("~/"));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (UserQuery.UsernameExists(model.Username))
        {
            ModelState.AddModelError("Username", "Username is already taken.");
            return View(model);
        }
        
        if (UserQuery.EmailExists(model.Email))
        {
            ModelState.AddModelError("Email", "Email is already registered.");
            return View(model);
        }

        User user = UserQuery.CreateUser(model);

        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, user.Username),
            new (ClaimTypes.Email, user.Email),
            new (ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        // Since we're only creating author accounts, directly redirect to author dashboard
        return RedirectToAction("Dashboard", "Author");
    }
}