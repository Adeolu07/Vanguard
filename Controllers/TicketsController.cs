using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;

namespace _Tripfinity.Controllers;

public class TicketController : Controller
{
    private readonly AppDbContext _context;
    private readonly ITicketService _ticketService;
    private readonly IBookingService _bookingService;

    public TicketController(AppDbContext context, ITicketService ticketService, IBookingService bookingService)
    {
        _context = context;
        _ticketService = ticketService;
        _bookingService = bookingService;
    }

    // GET: /Ticket/Index
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("userId");
    
        if (userId == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }
    
        // Load bookings for this user
        var bookings = await _context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.TaxiTrip)
            .Include(b => b.RailwayTrip)
            .Where(b => b.UserId == userId.Value)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    
        // Load tickets for this user
        var tickets = await _context.Tickets
            .Where(t => t.PassengerId == userId.Value)
            .ToListAsync();
    
        // Pass both bookings and tickets to the view
        ViewBag.Tickets = tickets;
        ViewBag.UserName = HttpContext.Session.GetString("Username");
    
        return View("Ticket", bookings);
    }

    // GET: /Ticket/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = HttpContext.Session.GetInt32("userId");

        if (userId == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        var booking = await _context.Bookings
            .Include(b => b.BusTrip)
            .Include(b => b.TaxiTrip)
            .Include(b => b.RailwayTrip)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId.Value);

        if (booking == null)
        {
            return NotFound();
        }

        var ticket = await _ticketService.GetTicketByBookingAsync(booking.Id);
        ViewBag.Ticket = ticket;
        ViewBag.QRCodeText = ticket?.TicketReference ?? $"TKT-{booking.Id}";
        ViewBag.UserName = HttpContext.Session.GetString("Username");

        return View("Details", booking);
    }

    // GET: /Ticket/QrCode/5  (bookingId) -> PNG of the issued ticket's QR reference
    [HttpGet]
    public async Task<IActionResult> QrCode(int id)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
            return RedirectToAction("SignIn", "Auth");

        var ticket = await _ticketService.GetTicketByBookingAsync(id);
        if (ticket == null || ticket.PassengerId != userId.Value)
            return NotFound();

        var png = _ticketService.GenerateQrCode(ticket.TicketReference);
        return File(png, "image/png");
    }

    // POST: /Ticket/Cancel  -> passenger-initiated cancellation (refund when eligible)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int bookingId, string? reason)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
            return RedirectToAction("SignIn", "Auth");

        var result = await _bookingService.CancelBookingAsync(
            bookingId, userId.Value, reason ?? "Cancelled by passenger");

        if (result.Success)
            TempData["Success"] = result.Message;
        else
            TempData["Error"] = result.Message;

        return RedirectToAction("Index");
    }
}
