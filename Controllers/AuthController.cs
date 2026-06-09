using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user!.Id, token = WebUtility.UrlEncode(token) }, Request.Scheme);
        await _emailService.SendEmailAsync(user.Email, "Confirm your Tripfinity account", $"Click here to confirm your email: {confirmationLink}");

        TempData["Success"] = "Marshal registration successful! Please check your email to confirm your account.";
        return RedirectToAction("SignIn");
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
            SetSignInError(await _authService.GetUserByEmailAsync(model.Email));
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
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _authService.EmailExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Email already exists");
            return View(model);
        }

        var user = await _authService.SignUpAsync(model.Email, model.Password, model.FirstName, model.LastName, "Passenger");

        var token = await _authService.GenerateEmailConfirmationTokenAsync(user!);
        var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user!.Id, token = WebUtility.UrlEncode(token) }, Request.Scheme);
        await _emailService.SendEmailAsync(user.Email, "Confirm your Tripfinity account", $"Click here to confirm your email: {confirmationLink}");

        TempData["Success"] = "Passenger registration successful! Please check your email to confirm your account.";
        return RedirectToAction("SignIn");
    }

    [HttpGet]
    public IActionResult SignIn()
    {
        if (HttpContext.Session.GetString("UserEmail") != null)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _authService.SignInAsync(model.Email, model.Password);
        if (user == null)
        {
            SetSignInError(await _authService.GetUserByEmailAsync(model.Email));
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

        TempData[success ? "Success" : "Error"] = success
            ? "Email confirmed successfully! You can now sign in."
            : "Invalid or expired confirmation link.";

        return RedirectToAction(success ? "SignIn" : "SignUp");
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
        await _emailService.SendEmailAsync(model.Email, "Reset your Tripfinity password", $"Click here to reset your password: {resetLink}");

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
        TempData[success ? "Success" : "Error"] = success
            ? "Password reset successful! You can now sign in."
            : "Invalid or expired reset link.";

        return RedirectToAction(success ? "SignIn" : "ResetPassword");
    }

    [HttpPost]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        var result = await _authService.ResendConfirmationAsync(email, _emailService, Url, Request.Scheme);
        TempData[result.Key] = result.Value;
        return RedirectToAction("SignIn");
    }

    // 🔹 Private helper method for error handling
    private void SetSignInError(User? existingUser)
    {
        if (existingUser?.LockoutEnd > DateTime.UtcNow)
            TempData["Error"] = $"Account locked until {existingUser.LockoutEnd.Value.ToLocalTime()}";
        else if (existingUser != null && !existingUser.EmailConfirmed)
            TempData["Error"] = "Email not confirmed. Please check your inbox.";
        else
            TempData["Error"] = "Invalid credentials.";
    }
}
