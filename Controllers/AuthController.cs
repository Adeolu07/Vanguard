using _Tripfinity.Interfaces;
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
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.SignInAsync(model.Email, model.Password, "Passenger");

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        _authService.SetUserSession(HttpContext, result.User);
        return RedirectToAction("Index", "Home");
    }

    // ✅ Marshal Sign In (points to Marshal-SignIn.cshtml)
    [HttpGet]
    public IActionResult MarshalSignIn()
    {
        if (HttpContext.Session.GetInt32("userId") != null)
            return RedirectToAction("Index", "Home");
        return View("Marshal-SignIn");
    }

    [HttpPost]
    public async Task<IActionResult> MarshalSignIn(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View("Marshal-SignIn", model);

        var result = await _authService.SignInAsync(model.Email, model.Password, "Marshal");

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError("", result.Message);
            return View("Marshal-SignIn", model);
        }

        _authService.SetUserSession(HttpContext, result.User);
        return RedirectToAction("Index", "Home");
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
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.SignUpAsync(
            model.Email, model.Password, model.FirstName, model.LastName, model.PhoneNumber, "Passenger"
        );

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        var confirmationLink = $"{Request.Scheme}://{Request.Host}/account/ConfirmEmail?" +
                               $"userId={result.User.Id}&token={result.User.EmailConfirmationToken}";
        await _emailService.SendConfirmationEmailAsync(result.User.Email, confirmationLink);

        return RedirectToAction("Index", "Home");
    }

    // ✅ Marshal Sign Up (points to Marshal-SignUp.cshtml)
    [HttpGet]
    public IActionResult MarshalSignUp()
    {
        if (HttpContext.Session.GetInt32("userId") != null)
            return RedirectToAction("Index", "Home");
        return View("Marshal-SignUp");
    }

    [HttpPost]
    public async Task<IActionResult> MarshalSignUp(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View("Marshal-SignUp", model);

        var result = await _authService.SignUpAsync(
            model.Email, model.Password, model.FirstName, model.LastName, model.PhoneNumber, "Marshal"
        );

        if (!result.Success || result.User == null)
        {
            ModelState.AddModelError("", result.Message);
            return View("Marshal-SignUp", model);
        }

        var confirmationLink = $"{Request.Scheme}://{Request.Host}/account/ConfirmEmail?" +
                               $"userId={result.User.Id}&token={result.User.EmailConfirmationToken}";
        await _emailService.SendConfirmationEmailAsync(result.User.Email, confirmationLink);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
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

    public IActionResult Logout()
    {
        _authService.ClearUserSession(HttpContext);
        return RedirectToAction("Index", "Home");
    }
}
