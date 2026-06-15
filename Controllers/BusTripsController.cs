using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using _Tripfinity.Services;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class BusTripsController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly AppDbContext _context;

    public BusTripsController(AppDbContext context, IBookingService bookingService)
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

        var trip = await _context.BusTrips.FindAsync(tripId);
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

        var booking = await _bookingService.BookBusAsync(tripId, seats, userEmail);
        if (booking == null)
        {
            TempData["Error"] = "Bus booking failed.";
            return RedirectToAction("Book", new { tripId });
        }

        TempData["Success"] = "Bus booking confirmed!";
        return RedirectToAction("Confirmation", new { id = booking.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (userEmail == null) return RedirectToAction("SignIn", "Auth");

        var booking = await _bookingService.GetBookingAsync(id, "Bus");
        if (booking == null) return NotFound();

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View(booking);
    }
}
