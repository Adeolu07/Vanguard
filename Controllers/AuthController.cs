using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }
    // update to real db
    
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
        {
            return View(model);
        }

        if (await _context.Users.AnyAsync(user => user.Email == model.Email))
        {
            ModelState.AddModelError("Email","Email already exists");
            return View(model);
        }

        var user = new User
        {
            Email = model.Email,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.Now
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        // Successful registration
        
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("Username", user.FirstName+user.LastName);
        
        
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
        
        var user = await _context.Users.FirstOrDefaultAsync(user => 
            user.Email == model.Email && 
            user.Password == model.Password);
        
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }
        
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("Username", user.FirstName + " " +  user.LastName);
        
        TempData["Success"] = $"Welcome back, {user.FirstName} {user.LastName}!";
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult SignOut()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You've been signed out";
        return RedirectToAction("SignIn", "Auth");
    }

}