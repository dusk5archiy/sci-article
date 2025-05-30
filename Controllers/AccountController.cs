using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SciArticle.Models.Back;
using SciArticle.Models.Front.User;
using SciArticle.Models.Object;
using SciArticle.Models.Utilities;

namespace SciArticle.Controllers;

public class AccountController : Controller
{
    public AccountController() { }

    public IActionResult AccessDenied(string returnUrl = null!)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
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
            ModelState.AddModelError(string.Empty, "Đăng nhập không thành công");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            RedirectUri = returnUrl,
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        if (user.Role == UserRole.Author)
        {
            return RedirectToAction("Dashboard", "Author");
        }
        if (user.Role == UserRole.Admin)
        {
            return RedirectToAction("Dashboard", "Admin");
        }

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
            ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
            return View(model);
        }

        if (UserQuery.EmailExists(model.Email))
        {
            ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống");
            return View(model);
        }

        User user = UserQuery.CreateUser(model);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity)
        );

        return RedirectToAction("Dashboard", "Author");
    }
}

