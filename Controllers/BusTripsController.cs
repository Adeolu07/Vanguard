using _Tripfinity.Models;
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

    // GET: HTML View
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserEmail") == null) return RedirectToAction("SignIn", "Auth");

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View();
    }

    // GET: /BusTrips/Book/5
    [HttpGet]
    public async Task<IActionResult> Book(int tripId)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        var trip = await _context.BusTrips.FindAsync(tripId);
        if (trip == null) return NotFound();

        ViewBag.Trip = trip;
        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View();
    }

    // POST: /BusTrips/Book
    [HttpPost]
    public async Task<IActionResult> Book(int tripId, int seats)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        var userEmail = HttpContext.Session.GetString("UserEmail");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        var trip = await _context.BusTrips.FindAsync(tripId);
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
            BusTripId = tripId,
            TransportType = "Bus",
            NumberOfSeats = seats,
            TotalAmount = trip.Price * seats,
            Status = "Confirmed",
            BookingDate = DateTime.Now
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Bus booking confirmed!";
        return RedirectToAction("Confirmation", new { id = booking.Id });
    }

    // GET: /BusTrips/Confirmation/5
    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        var booking = await _context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View(booking);
    }
}