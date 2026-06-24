using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Controllers;

public class WalletController : Controller
{
    private readonly ILogger<WalletController> _logger;
    private readonly IWalletService _walletService;
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;

    public WalletController(ILogger<WalletController> logger, IWalletService walletService, IAuthService authService, AppDbContext context)
    {
        _walletService = walletService;
        _logger = logger;
        _authService = authService;
        _context = context;
    }

    [HttpGet("/Wallet/Debug")]
    public async Task<IActionResult> Debug()
    {
        var userId = HttpContext.Session.GetInt32("userId");
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        var wallet = await _context.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId);

        return Json(new
        {
            userId,
            userWalletId = user?.UserWalletId,
            walletCustomerId = wallet?.CustomerId,
            walletBalance = wallet?.Balance,
            match = user?.UserWalletId == wallet?.CustomerId
        });
    }

    [HttpGet("/Wallet")]
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
            return RedirectToAction("DecisionLogin", "Auth");

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return RedirectToAction("DecisionLogin", "Auth");

        if (string.IsNullOrEmpty(user.UserWalletId))
        {
            ViewBag.HasWallet = false;
            return View();
        }

        var request = new GetBalanceRequest { CustomerId = user.UserWalletId };
        var response = await _walletService.GetBalanceAsync(request);

        ViewBag.HasWallet = true;
        return View(response);
    }

    [HttpPost("/Wallet/Initialize")]
    public async Task<IActionResult> InitializeWalletView()
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
            return RedirectToAction("DecisionLogin", "Auth");

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return RedirectToAction("DecisionLogin", "Auth");

        if (!string.IsNullOrEmpty(user.UserWalletId))
            return RedirectToAction("Index");

        var createRequest = new CreateWalletRequest
        {
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? ""
        };

        var response = await _walletService.CreateWalletAsync(createRequest);

        if (response?.ResponseHeader?.ResponseCode == "00" && response.AccountDetails != null)
        {
            await _walletService.SaveUserWalletIdAsync(user.Id, response.AccountDetails.CustomerId);
        }

        return RedirectToAction("Index");
    }

    [HttpGet("/Wallet/History")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || string.IsNullOrEmpty(user.UserWalletId))
            return Unauthorized();

        var transactions = await _walletService.GetTransactionHistoryAsync(user.UserWalletId);
        return Json(transactions);
    }

    [HttpPost("/Wallet/FundForm")]
    public async Task<IActionResult> FundWalletFromForm([FromForm] decimal amount)
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
            return RedirectToAction("Index");

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || string.IsNullOrEmpty(user.UserWalletId) || amount <= 0)
            return RedirectToAction("Index");

        HttpContext.Session.SetString("pendingAmount", amount.ToString());
        HttpContext.Session.SetString("pendingTraceId", Guid.NewGuid().ToString("N"));

        ViewBag.Pending = true;
        ViewBag.Amount = amount;
        return View("PaymentResult");
    }

    [HttpGet("/Wallet/VerifyPayment")]
    public async Task<IActionResult> VerifyPayment()
    {
        var userId = HttpContext.Session.GetInt32("userId");
        if (userId == null)
            return Json(new { success = false });

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        var amountStr = HttpContext.Session.GetString("pendingAmount");
        var traceId = HttpContext.Session.GetString("pendingTraceId");

        if (user == null || string.IsNullOrEmpty(user.UserWalletId)
            || string.IsNullOrEmpty(amountStr) || string.IsNullOrEmpty(traceId))
            return Json(new { success = false });

        var amount = decimal.Parse(amountStr);

        var request = new CreditWalletRequest
        {
            CustomerId = user.UserWalletId,
            Amount = amount,
            Description = "Web UI Wallet Funding",
            TraceId = traceId
        };

        try
        {
            var result = await _walletService.CreditWalletAsync(request);
            var success = result?.ResponseHeader?.ResponseCode == "00";

            HttpContext.Session.Remove("pendingAmount");
            HttpContext.Session.Remove("pendingTraceId");

            return Json(new { success, amount });
        }
        catch
        {
            return Json(new { success = false, amount });
        }
    }

    [HttpGet("/Wallet/PaymentResult")]
    public IActionResult PaymentResult(bool success, decimal amount)
    {
        ViewBag.Success = success;
        ViewBag.Amount = amount;
        return View();
    }
}