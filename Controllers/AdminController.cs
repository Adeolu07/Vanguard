using _Tripfinity.Interfaces;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

[Route("admin")]
[RequireAuth]
public class AdminController : ParentController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    private async Task<IActionResult> RequireAdmin()
    {
        if (!IsAuthenticated)
            return RedirectToAction("SignIn", "Auth");

        bool isAdmin = await _adminService.IsAdminAsync(UserId!.Value);
        if (!isAdmin)
            return Forbid();

        return null!; // signals OK
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var guard = await RequireAdmin();
        if (guard != null) 
            return guard;

        return View();
    }
    
    [HttpGet("wallet")]
    public async Task<IActionResult> Wallet(int page = 1)
    {
        var guard = await RequireAdmin();
        if (guard != null) return guard;

        var walletId = await _adminService.GetAdminWalletIdAsync();
        if (walletId == null)
        {
            TempData["ErrorMessage"] = "Admin wallet not found.";
            return View("Wallet", new MarshalWalletViewModel { WalletId = "", Balance = 0, Transactions = new(), CurrentPage = 1, TotalPages = 1 });
        }

        var model = await _adminService.GetAdminWalletInfoAsync(walletId, page);
        return View(model);
    }

    [HttpGet("trips")]
    public async Task<IActionResult> Trips()
    {
        var guard = await RequireAdmin();
        if (guard != null) 
            return guard;

        var buses = await _adminService.GetAllBusTripsAsync();
        var rails = await _adminService.GetAllRailwayTripsAsync();
        var taxis = await _adminService.GetAllTaxiTripsAsync();

        ViewBag.BusTrips = buses;
        ViewBag.RailwayTrips = rails;
        ViewBag.TaxiTrips = taxis;

        return View();
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> Bookings()
    {
        var guard = await RequireAdmin();
        if (guard != null) return guard;

        var bookings = await _adminService.GetAllBookingsAsync();
        return View(bookings);
    }
}