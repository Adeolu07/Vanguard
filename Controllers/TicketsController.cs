using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _Tripfinity.Models.Data;

namespace _Tripfinity.Controllers;

public class TicketController : Controller
{
    private readonly AppDbContext _context;

    public TicketController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Ticket/Index
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        var userId = HttpContext.Session.GetInt32("UserId").Value;
        
        var tickets = await _context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.TaxiTrip)
            .Include(b => b.RailwayTrip)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        ViewBag.UserName = HttpContext.Session.GetString("Username");
        return View("Ticket",tickets);
    }

    // GET: /Ticket/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (HttpContext.Session.GetString("UserEmail") == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        var userId = HttpContext.Session.GetInt32("UserId").Value;
        
        var booking = await _context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.TaxiTrip)
            .Include(b => b.RailwayTrip)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (booking == null)
        {
            return NotFound();
        }

        // Generate fake QR code text (will replace with real QR later)
        ViewBag.QRCodeText = $"TKT-{booking.Id}-{booking.UserId}-{DateTime.Now.Ticks}";
        ViewBag.UserName = HttpContext.Session.GetString("Username");
        
        return View("Details", booking);
    }
}