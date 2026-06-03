using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class BusTripsController : Controller
{
    private readonly AppDbContext _context;

    public BusTripsController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        var trips = await _context.BusTrips
            .Where(trip => trip.IsActive && trip.DepartureTime > DateTime.Now)
            .OrderBy(trip => trip.DepartureTime)
            .ToListAsync();
        
        return View(trips);
    }
}