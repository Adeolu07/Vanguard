using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Services;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class RailwayTripsController : Controller
{
    private readonly IBookingService _bookingService; 
    private readonly AppDbContext _context;

    public RailwayTripsController(AppDbContext context, IBookingService bookingService)
    {
        _context = context;
        _bookingService = bookingService;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Book(int tripId)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        var trip = await _context.RailwayTrips.FindAsync(tripId);
        if (trip == null) return NotFound();

        ViewBag.Trip = trip;
        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Book(int tripId, int seats)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (userEmail == null) return RedirectToAction("SignIn", "Auth");

        var booking = await _bookingService.BookRailwayAsync(tripId, seats, userEmail);
        if (booking == null)
        {
            TempData["Error"] = "Railway booking failed.";
            return RedirectToAction("Book", new { tripId });
        }

        TempData["Success"] = "Railway booking confirmed!";
        return RedirectToAction("Confirmation", new { id = booking.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (userEmail == null) return RedirectToAction("SignIn", "Auth");

        var booking = await _bookingService.GetBookingAsync(id, "Railway");
        if (booking == null) return NotFound();

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View(booking);
    }
}
