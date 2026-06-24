using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AuthController(IAuthService authService, IEmailService emailService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult DecisionLogin() => View();

    [HttpGet]
    public IActionResult DecisionSignup() => View();

    [HttpGet]
    public IActionResult SignIn()
    {
        if (HttpContext.Session.GetInt32("userId") != null)
            return RedirectToAction("Dashboard", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(LoginViewModel model)
    {
        if (!ModelState.IsValid) 
            return View(model);

        var result = await _authService.SignInAsync(model.Email, model.Password);

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        _authService.SetUserSession(HttpContext, result.User);
        await HttpContext.Session.CommitAsync();
        return RedirectToAction("Dashboard", "Home");
    }

    [HttpGet]
    public IActionResult MarshalSignIn()
    {
        if (HttpContext.Session.GetInt32("userId") != null)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> MarshalSignIn(LoginViewModel model)
    {
        if (!ModelState.IsValid) 
            return View(model);

        var result = await _authService.SignInAsync(model.Email, model.Password);

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        _authService.SetUserSession(HttpContext, result.User);
        return RedirectToAction("Index", "Marshal");
        // change to marshal dashboard
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        if (HttpContext.Session.GetInt32("userId") != null)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(RegisterViewModel model)
    {
        if (!ModelState.IsValid) 
            return View(model);

        var result = await _authService.SignUpAsync(model);

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        var confirmationLink = $"{Request.Scheme}://{Request.Host}/auth/ConfirmEmail?" +
                               $"userId={result.User.Id}&token={result.User.EmailConfirmationToken}";
        await _emailService.SendConfirmationEmailAsync(result.User.Email, confirmationLink);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult MarshalSignUp()
    {
        if (HttpContext.Session.GetInt32("userId") != null)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> MarshalSignUp(MarshalRegisterRequest model)
    {
        if (!ModelState.IsValid) 
            return View(model);

        var result = await _authService.RegisterMarshalAsync(model);

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }
        
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
    {
        var result = await _authService.ConfirmationEmailAsync(userId, token);
        if (!result)
        {
            TempData["ErrorMessage"] = "Invalid or expired email confirmation token.";
            return RedirectToAction("SignIn");
        }

        TempData["SuccessMessage"] = "Email confirmation and wallet creation successful.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Email is required.");
            return View();
        }

        var result = await _authService.ForgotPasswordAsync(email);
        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("ForgotPassword");
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return BadRequest("Invalid request.");

        var model = new ResetPasswordViewModel { Email = email, Token = token };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("SignIn");
        }

        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    public IActionResult Logout()
    {
        _authService.ClearUserSession(HttpContext);
        return RedirectToAction("Index", "Home");
    }
}
