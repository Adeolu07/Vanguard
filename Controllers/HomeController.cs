using System.Diagnostics;
using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public HomeController(IAuthService authService, AppDbContext context)
    {
        _context = context;
        _authService = authService;
    }

    [Route("/home")]
    [Route("/")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
            return View("Index");

        var upcomingTrips = await _context.Bookings
            .Include(b => b.BusTrip)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .Take(3)
            .ToListAsync();

        var user = _authService.GetCurrentUser(HttpContext);
        ViewBag.FirstName = user!.FirstName;

        return View("Dashboard", upcomingTrips);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public IActionResult Wallet()
    {
        if (HttpContext.Session.GetInt32("userId") == null)
            return RedirectToAction("SignIn", "Auth");
        return View("~/Views/Wallet/Index.cshtml");
    }
}