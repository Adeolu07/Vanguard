using _Tripfinity.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class TicketController : ParentController
{
    private readonly IPassengerService _passengerService;
    private readonly ITicketService _ticketService;
    private readonly IBookingService _bookingService;

    public TicketController(IPassengerService passengerService, ITicketService ticketService,
        IBookingService bookingService)
    {
        _passengerService = passengerService;
        _ticketService = ticketService;
        _bookingService = bookingService;
    }

    // GET: /Ticket/Index
    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated) return RedirectToLogin();

        var model = await _passengerService.GetTicketIndexAsync(UserId!.Value);
        return View(model);
    }

    // GET: /Ticket/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (!IsAuthenticated) return RedirectToLogin();

        var model = await _passengerService.GetBookingDetailAsync(id, UserId!.Value);
        if (model is null) return View($"Error");

        return View("Details", model);
    }

    // GET: /Ticket/QrCode/5
    [HttpGet]
    public async Task<IActionResult> QrCode(int id)
    {
        if (!IsAuthenticated) return RedirectToLogin();

        var ticket = await _ticketService.GetTicketByBookingAsync(id);
        if (ticket is null || ticket.PassengerId != UserId!.Value)
            return NotFound();

        var png = _ticketService.GenerateQrCode(ticket.TicketReference);
        return File(png, "image/png");
    }

    // POST: /Ticket/Cancel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int bookingId, string? reason)
    {
        if (!IsAuthenticated) return RedirectToLogin();

        var result = await _bookingService.CancelBookingAsync(
            bookingId, UserId!.Value, reason ?? "Cancelled by passenger");

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction("Details", new{id = bookingId});
    }
}