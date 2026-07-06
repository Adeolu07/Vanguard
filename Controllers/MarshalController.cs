using System.Net;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

public class MarshalController : ParentController
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;
    
    public  MarshalController(IAuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    public IActionResult Index()
    {
        var user = _authService.GetCurrentUser(HttpContext);

        if (user == null || user.Role != "Marshal")
            return RedirectToLogin();

        ViewBag.VehicleId = user.VehicleId;
        ViewBag.VehicleType = user.VehicleType;
        ViewBag.TripsCount = 5;               // fetch from DB later
        ViewBag.ScannedCount = 12;
        ViewBag.PendingCount = 3;
        ViewBag.RecentTickets = new List<object>();
        return View();
    }

    [HttpGet]
    public IActionResult Scan()
    {
        var user = _authService.GetCurrentUser(HttpContext);
        
        if(user == null || user.Role != "Marshal")
            return  RedirectToLogin();
        return View();
    }
    
    [HttpGet]
    public async Task<IActionResult> Trips()
    {
        var user = _authService.GetCurrentUser(HttpContext);
        if (user == null || user.Role != "Marshal")
            return RedirectToLogin();

        ViewBag.VehicleType = user.VehicleType;
        ViewBag.Trips = new List<object>(); // placeholder
        return View();
    }
    
    [HttpGet]
    public async Task<IActionResult> Cancel()
    {
        var user = _authService.GetCurrentUser(HttpContext);
        if (user == null || user.Role != "Marshal")
            return RedirectToLogin();

        ViewBag.VehicleType = user.VehicleType;
        ViewBag.Trips = new List<object>();
        return View();
    }
}