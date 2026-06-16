using _Tripfinity.Models;
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

    public IActionResult Index()
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Book(int tripId)
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        var trip = await _context.RailwayTrips.FindAsync(tripId);
        if (trip == null)
        {
            return NotFound("Trip not found.");
        }
        ViewBag.Trip = trip;
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Book(int tripId, int seats)
    {
        if (HttpContext.Session.GetInt32("userId") == null)
            return RedirectToAction("SignIn", "Auth");

        var userEmail = HttpContext.Session.GetString("UserEmail");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        var trip = await _context.RailwayTrips.FindAsync(tripId);
        if (trip == null) return NotFound();

        if (seats > trip.AvailableSeats)
        {
            TempData["Error"] = "Not enough seats available.";
            return RedirectToAction("Book", new { tripId });
        }

        trip.AvailableSeats -= seats;

        var booking = new Booking
        {
            UserId = user.Id,
            RailwayTripId = tripId,
            TransportType = "Railway",
            NumberOfSeats = seats,
            TotalAmount = trip.Price * seats,
            Status = "Confirmed",
            BookingDate = DateTime.Now
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Railway booking confirmed!";
        return RedirectToAction("Confirmation", new { id = booking.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        if (HttpContext.Session.GetInt32("userId") == null)
            return RedirectToAction("SignIn", "Auth");

        var booking = await _context.Bookings
            .Include(b => b.RailwayTrip)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }
        
        return View(booking);
    }
}