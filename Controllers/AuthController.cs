using _Tripfinity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Services;

namespace _Tripfinity.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult SelectRole() => View();

    [HttpGet]
    public IActionResult MarshalSignUp() => View();

    [HttpPost]
    public async Task<IActionResult> MarshalSignUp(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _authService.EmailExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Email already exists");
            return View(model);
        }
        var user = await _authService.SignUpAsync(model.Email, model.Password, model.FirstName, model.LastName, "Marshal");
        _authService.SetUserSession(HttpContext, user!);
        TempData["Success"] = "Marshal registration successful!";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult MarshalSignIn() => View();

    [HttpPost]
    public async Task<IActionResult> MarshalSignIn(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _authService.SignInAsync(model.Email, model.Password);
        if (user == null || user.Role != "Marshal")
        {
            ModelState.AddModelError("", "Invalid Marshal credentials");
            return View(model);
        }
        _authService.SetUserSession(HttpContext, user);
        TempData["Success"] = $"Welcome Marshal {user.FirstName}!";
        return RedirectToAction("Index", "Home");
    }


    [HttpGet]
    public IActionResult SignUp()
    {
        if (HttpContext.Session.GetString("UserEmail") != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> SignUp(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        if (await _authService.EmailExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Email already exists");
            return View(model);
        }
        await _authService.SignUpAsync(model.Email, model.Password, model.FirstName, model.LastName, "Passenger");
        var user = await _authService.SignInAsync(model.Email, model.Password);
        
        _authService.SetUserSession(HttpContext, user!);
        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    public IActionResult SignIn()
    {
        if (HttpContext.Session.GetString("UserEmail") != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> SignIn(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await _authService.SignInAsync(model.Email, model.Password);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }
        _authService.SetUserSession(HttpContext, user);
        TempData["Success"] = $"Welcome back, {user.FirstName} {user.LastName}!";
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult Logout()
    {
        _authService.ClearUserSession(HttpContext);
        TempData["Success"] = "You've been signed out";
        return RedirectToAction("SignIn", "Auth");
    }
}