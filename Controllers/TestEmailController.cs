using Microsoft.AspNetCore.Mvc;
using _Tripfinity.Interfaces;

namespace _Tripfinity.Controllers
{
    public class TestEmailController : Controller
    {
        private readonly IEmailService _emailService;

        public TestEmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> SendTest()
        {
            await _emailService.SendEmailAsync(
                "yourpersonalemail@example.com",   // 👈 replace with your real email
                "Tripfinity Test Email",
                "This is a test email sent via SendGrid from Tripfinity."
            );

            return Content("Test email sent! Check your inbox.");
        }
    }
}
