using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class RailwayTripsController : Controller
{
    private readonly AppDbContext _context;

    public RailwayTripsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: HTML View
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View();
    }
}