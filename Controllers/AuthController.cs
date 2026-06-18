using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;
    private  readonly IEmailService _emailService;
    private readonly IWalletService _walletService;

    public AuthController(AppDbContext context, IAuthService authService, IEmailService emailService, IWalletService walletService)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
        _walletService = walletService;
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

            var token = result.User.EmailConfirmationToken;
            var confirmationLink = Url.Action("ConfirmEmail", "Auth", 
                new { token, email = result.User.Email }, Request.Scheme);
            
            await _emailService.SendConfirmationEmailAsync(result.User.Email, confirmationLink);
            

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(model);
        }
    }


    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token, string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found";
            return RedirectToAction("SignIn");
        }

        if (user.IsEmailConfirmed)
        {
            TempData["ErrorMessage"] = "Email already confirmed";
            return RedirectToAction("SignIn");
        }

        if (user.EmailConfirmationToken != token)
        {
            TempData["ErrorMessage"] = "Invalid confirmation token";
            return RedirectToAction("SignIn");
        }

        // Confirm email
        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        
        _walletService.CreateWallet(user.FirstName, user.LastName,user.Email);
        
        return RedirectToAction("SignIn");
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