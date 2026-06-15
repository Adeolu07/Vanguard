using _Tripfinity.Models.Data;
using _Tripfinity.Services;
using _Tripfinity.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class TaxiTripsController : Controller
{
    private readonly IBookingService _bookingService; 
    private readonly AppDbContext _context;

    public TaxiTripsController(AppDbContext context, IBookingService bookingService)
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

        var trip = await _context.TaxiTrips.FindAsync(tripId);
        if (trip == null) return NotFound();

        ViewBag.Trip = trip;
        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View("Book", trip);
    }

    [HttpPost]
    public async Task<IActionResult> Book(int tripId, int seats)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (userEmail == null) return RedirectToAction("SignIn", "Auth");

        var booking = await _bookingService.BookTaxiAsync(tripId, seats, userEmail);
        if (booking == null)
        {
            TempData["Error"] = "Taxi booking failed.";
            return RedirectToAction("Book", new { tripId });
        }

        TempData["Success"] = "Taxi booking confirmed!";
        return RedirectToAction("Confirmation", new { id = booking.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (userEmail == null) return RedirectToAction("SignIn", "Auth");

        var booking = await _bookingService.GetBookingAsync(id, "Taxi");
        if (booking == null) return NotFound();

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View(booking);
    }
}
