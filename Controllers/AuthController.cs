using _Tripfinity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Services;

namespace _Tripfinity.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public AuthController(IAuthService authService, IEmailService emailService)
    {
        _authService = authService;
        _emailService = emailService;
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

        var token = await _authService.GenerateEmailConfirmationTokenAsync(user!);
        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Auth",
            new { userId = user!.Id, token = WebUtility.UrlEncode(token) }, 
            Request.Scheme
        );
        await _emailService.SendEmailAsync(user.Email, "Confirm your Tripfinity account",
            $"Click here to confirm your email: {confirmationLink}");


        TempData["Success"] = "Marshal registration successful! Please check your email to confirm your account.";
        return RedirectToAction("SignIn");
    }

    [HttpGet]
    public IActionResult MarshalSignIn() => View();

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> MarshalSignIn(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _authService.SignInAsync(model.Email, model.Password);
        if (user == null || user.Role != "Marshal")
        {
            var existingUser = await _authService.GetUserByEmailAsync(model.Email);
            if (existingUser != null && existingUser.LockoutEnd.HasValue && existingUser.LockoutEnd > DateTime.UtcNow)
            {
                TempData["Error"] = $"Account locked until {existingUser.LockoutEnd.Value.ToLocalTime()}";
            }
            else if (existingUser != null && !existingUser.EmailConfirmed)
            {
                TempData["Error"] = "Email not confirmed. Please check your inbox or resend confirmation.";
            }
            else
            {
                TempData["Error"] = "Invalid credentials.";
            }
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

        var user = await _authService.SignUpAsync(model.Email, model.Password, model.FirstName, model.LastName, "Passenger");

        var token = await _authService.GenerateEmailConfirmationTokenAsync(user!);
        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Auth",
            new { userId = user!.Id, token = WebUtility.UrlEncode(token) }, 
            Request.Scheme
        );
        await _emailService.SendEmailAsync(user.Email, "Confirm your Tripfinity account",
            $"Click here to confirm your email: {confirmationLink}");

        TempData["Success"] = "Passenger registration successful! Please check your email to confirm your account.";
        return RedirectToAction("SignIn");
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
            var existingUser = await _authService.GetUserByEmailAsync(model.Email);
            if (existingUser != null && existingUser.LockoutEnd.HasValue && existingUser.LockoutEnd > DateTime.UtcNow)
            {
                TempData["Error"] = $"Account locked until {existingUser.LockoutEnd.Value.ToLocalTime()}";
            }
            else if (existingUser != null && !existingUser.EmailConfirmed)
            {
                TempData["Error"] = "Email not confirmed. Please check your inbox or resend confirmation.";
            }
            else
            {
                TempData["Error"] = "Invalid credentials.";
            }
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

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var decodedToken = Uri.UnescapeDataString(token);
        var success = await _authService.ConfirmEmailAsync(userId, decodedToken);

        if (success)
        {
            TempData["Success"] = "Email confirmed successfully! You can now sign in.";
            return RedirectToAction("SignIn");
        }

        TempData["Error"] = "Invalid or expired confirmation link.";
        return RedirectToAction("SignUp");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = await _authService.GeneratePasswordResetTokenAsync(model.Email);
        if (token == null)
        {
            ModelState.AddModelError("", "Email not found");
            return View(model);
        }

        var resetLink = Url.Action("ResetPassword", "Auth", new { email = model.Email, token }, Request.Scheme);
        await _emailService.SendEmailAsync(model.Email, "Reset your Tripfinity password",
            $"Click here to reset your password: {resetLink}");

        TempData["Success"] = "Password reset link sent! Check your email.";
        return RedirectToAction("SignIn");
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token) =>
        View(new ResetPasswordViewModel { Email = email, Token = token });

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var success = await _authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
        if (success)
        {
            TempData["Success"] = "Password reset successful! You can now sign in.";
            return RedirectToAction("SignIn");
        }

        TempData["Error"] = "Invalid or expired reset link.";
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        var result = await _authService.ResendConfirmationAsync(email, _emailService, Url, Request.Scheme);
        TempData[result.Key] = result.Value;
        return RedirectToAction("SignIn");
    }

}
