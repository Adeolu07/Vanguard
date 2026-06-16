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

    [Route("/home")]
    [Route("/")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
            return View("Index");
        
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