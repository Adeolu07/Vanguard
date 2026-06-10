using System.Diagnostics;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
            return View("Landing");
        ViewBag.UserName = HttpContext.Session.GetString("UserName");

        var userId = HttpContext.Session.GetInt32("UserId").Value;
        var recentBookings = await _context.Bookings
            .Include(b => b.BusTrip)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .Take(5)
            .ToListAsync();

        return View("Dashboard", recentBookings);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}