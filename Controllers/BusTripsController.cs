using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class BusTripsController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;

    public BusTripsController(AppDbContext context, IBookingService bookingService, IWalletService walletService)
    {
        _context = context;
        _bookingService = bookingService;
        _walletService = walletService;
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

        var trip = await _context.BusTrips.FindAsync(tripId);
        if (trip == null)
        {
            return NotFound("Trip not found");
        }

        ViewBag.Trip = trip;
        ViewBag.UserId = HttpContext.Session.GetInt32("userId");
        return View("Book", trip);
    }

    [HttpPost]
    public async Task<IActionResult> Book(int tripId, int seats)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        var result = await _bookingService.BookBusAsync(tripId, seats, userId.Value);
        if (!result.Success)
        {
            if (result.Status == "InsufficientFunds")
                TempData["Warning"] = result.Message;
            else
                TempData["Error"] = result.Message;
            return RedirectToAction("Book", new { tripId });
        }

        TempData["Success"] = result.Message;
        return RedirectToAction("Confirmation", new { id = result.Booking!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null) 
            return RedirectToAction("SignIn", "Auth");

        var booking = await _bookingService.GetBookingAsync(id, "Bus");
        if (booking == null) 
            return NotFound("Bus booking not found.");

        if (!string.IsNullOrEmpty(booking.PaymentTransactionId))
        {
            var transReq = new GetTransactionRequest { TransactionId = booking.PaymentTransactionId };
            var transRes = await _walletService.GetTransactionAsync(transReq);
            ViewBag.PaymentTransaction = transRes?.TransactionDetails;
        }

        return View("Confirmation", booking);
    }
}
