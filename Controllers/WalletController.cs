using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data.Requests;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Controllers;

[Route("api/[controller]")]
public class WalletController : Controller
{
    private readonly ILogger<WalletController> _logger;
    private readonly IWalletService _walletService;
    private readonly IAuthService _authService;

    public WalletController(ILogger<WalletController> logger, IWalletService walletService, IAuthService authService)
    {
        _walletService = walletService;
        _logger = logger;
        _authService = authService;
    }

    [HttpGet("/Wallet")]
    public async Task<IActionResult> Index()
    {
        var user = _authService.GetCurrentUser(HttpContext);
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
        var user = _authService.GetCurrentUser(HttpContext);
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

    [HttpPost("/Wallet/FundForm")]
    public async Task<IActionResult> FundWalletFromForm([FromForm] decimal amount)
    {
        var user = _authService.GetCurrentUser(HttpContext);
        if (user == null || string.IsNullOrEmpty(user.UserWalletId) || amount <= 0)
            return RedirectToAction("Index");

        var request = new CreditWalletRequest
        {
            CustomerId = user.UserWalletId,
            Amount = amount,
            Description = "Web UI Wallet Funding",
            TraceId = Guid.NewGuid().ToString("N")
        };

        await _walletService.CreditWalletAsync(request);
        return RedirectToAction("Index");
    }

    [HttpPost("createWallet")]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        var response = await _walletService.CreateWalletAsync(request);
        return Ok(response);
    }

    [HttpPost("credit")]
    public async Task<IActionResult> CreditWallet([FromBody] CreditWalletRequest request)
    {
        var response = await _walletService.CreditWalletAsync(request);
        return Ok(response);
    }

    [HttpPost("debit")]
    public async Task<IActionResult> DebitWallet([FromBody] DebitWalletRequest request)
    {
        var response = await _walletService.DebitWalletAsync(request);
        return Ok(response);
    }

    [HttpPost("balance")]
    public async Task<IActionResult> GetBalance([FromBody] GetBalanceRequest request)
    {
        var response = await _walletService.GetBalanceAsync(request);
        return Ok(response);
    }
}