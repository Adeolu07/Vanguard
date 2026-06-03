using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _Tripfinity.Models;

namespace _Tripfinity.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }
        
        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    public IActionResult About()
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}