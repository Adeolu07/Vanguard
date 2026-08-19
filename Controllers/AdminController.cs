using _Tripfinity.Interfaces;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

[Route("admin")]
[ServiceFilter(typeof(AdminOnlyFilter))]
public class AdminController : ParentController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("wallet")]
    public async Task<IActionResult> Wallet(int page = 1)
    {
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
        var buses = await _adminService.GetAllTripsAsync<BusTrip>();
        var rails = await _adminService.GetAllTripsAsync<RailwayTrip>();
        var taxis = await _adminService.GetAllTripsAsync<TaxiTrip>();

        ViewBag.BusTrips = buses;
        ViewBag.RailwayTrips = rails;
        ViewBag.TaxiTrips = taxis;

        return View();
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> Bookings()
    {
        var bookings = await _adminService.GetAllBookingsAsync();
        return View(bookings);
    }
}