using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class RailwayBookingController : Controller
{
    private readonly AppDbContext _context;

    public RailwayBookingController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /RailwayBooking/Create?tripId=1
    public async Task<IActionResult> Create(int tripId)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        var trip = await _context.RailwayTrips.FindAsync(tripId);
        if (trip == null) return NotFound();

        ViewBag.Trip = trip;
        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View();
    }

    // POST: /RailwayBooking/Create
    [HttpPost]
    public async Task<IActionResult> Create(int tripId, int seats)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        var trip = await _context.RailwayTrips.FindAsync(tripId);
        if (trip == null) return NotFound();

        if (seats > trip.AvailableSeats)
        {
            ViewBag.Error = "Not enough seats available.";
            ViewBag.Trip = trip;
            return View();
        }

        var userEmail = HttpContext.Session.GetString("UserEmail");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null) return RedirectToAction("SignIn", "Auth");

        var booking = new RailwayBooking
        {
            UserId = user.Id,
            RailwayTripId = tripId,
            Amount = trip.Price * seats,
            Status = "Confirmed",
            BookingDate = DateTime.Now
        };

        trip.AvailableSeats -= seats;

        _context.RailwayBookings.Add(booking);
        await _context.SaveChangesAsync();

        return RedirectToAction("Confirmation", new { id = booking.Id });
    }

    // GET: /RailwayBooking/Confirmation/1
    public async Task<IActionResult> Confirmation(int id)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
            return RedirectToAction("SignIn", "Auth");

        var booking = await _context.RailwayBookings
            .Include(b => b.RailwayTrip)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View(booking);
    }
}