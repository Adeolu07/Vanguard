using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class TaxiTripsController : Controller
{
    private readonly AppDbContext _context;

    public TaxiTripsController(AppDbContext context)
    {
        _context = context;
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
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        var userEmail = HttpContext.Session.GetString("UserEmail");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        var trip = await _context.TaxiTrips.FindAsync(tripId);
        if (trip == null) return NotFound();

        if (seats > trip.MaxPassengers)
        {
            TempData["Error"] = $"Taxi can only take {trip.MaxPassengers} passengers.";
            return RedirectToAction("Book", new { tripId });
        }
        

        var booking = new Booking
        {
            UserId = user.Id,
            TaxiTripId = tripId,
            TransportType = "Taxi",
            NumberOfSeats = seats,
            TotalAmount = trip.Price,
            Status = "Confirmed",
            BookingDate = DateTime.Now
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Taxi booking confirmed!";
        return RedirectToAction("Confirmation", new { id = booking.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        var booking = await _context.Bookings
            .Include(b => b.TaxiTrip)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View(booking);
    }
}