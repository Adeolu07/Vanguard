using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
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
    private readonly ILogger _logger;

    public AuthController(AppDbContext context, IAuthService authService, IEmailService emailService, IWalletService walletService,  ILogger<AuthController> logger)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
        _walletService = walletService;
        _logger = logger;
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
        _logger.LogInformation("User creation initiated.");
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
                _logger.LogWarning("User creation failed.");
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            _logger.LogInformation("Account creation successful.");
            
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/account/ConfirmEmail?" +
                                   $"userId={result.User.Id}&token={result.User.EmailConfirmationToken}";
            
            await _emailService.SendConfirmationEmailAsync(result.User.Email, confirmationLink);
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("SignIn");
        }
    }


    [HttpGet]
    public async Task<IActionResult> ConfirmEmail([FromQuery]string userId, [FromQuery] string token)
    {
        var result = await _authService.ConfirmationEmailAsync(userId, token);
        if(!result)
        {
            TempData["ErrorMessage"] = "Invalid or expired email confirmation token.";
            return RedirectToAction("SignIn");
        }
        TempData["SuccessMessage"] = "Email confirmation and wallet creation successful.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(LoginViewModel model)
    {
        _logger.LogInformation("Sign in initiated.");
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
        _logger.LogInformation("Logout initiated.");
        _authService.ClearUserSession(HttpContext);
        return RedirectToAction("Index", "Home");
    }
}