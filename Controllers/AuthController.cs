using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;

    public AuthController(IAuthService authService, AppDbContext context)
    {
        _context = context;
        _authService = authService;
    }
    
    [HttpGet]
    public IActionResult SignIn()
    {
        if (HttpContext.Session.GetInt32("userId") != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        if (HttpContext.Session.GetInt32("userId") != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> SignUp(RegisterViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.SignUpAsync(
                model.Email,
                model.Password,
                model.FirstName,
                model.LastName
            );

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            
            _authService.SetUserSession(HttpContext, result.User!);
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(LoginViewModel model)
    {
        try
        {
            if (!ModelState.IsValid) 
                return View(model);
            
            var result = await _authService.SignInAsync(model.Email, model.Password);
            
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            
            _authService.SetUserSession(HttpContext, result.User!);
            return RedirectToAction("Index", "Home");
        }
        
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(model);
        }
    }

    public IActionResult Logout()
    {
        _authService.ClearUserSession(HttpContext);
        return RedirectToAction("Index", "Home");
    }
}