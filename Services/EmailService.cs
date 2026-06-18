using _Tripfinity.Interfaces;
using Resend;

namespace _Tripfinity.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendConfirmationEmailAsync(string email, string confirmationLink)
    {
        var apiKey = _config["Resend:ApiKey"];
        var fromEmail = _config["Resend:FromEmail"];
        
        IResend resend = ResendClient.Create(apiKey);

        var htmlContent = $@"
            <h2>Welcome to Tripfinity!</h2>
            <p>Please confirm your email by clicking the link below:</p>
            <a href='{{confirmationLink}}'>Confirm Email</a>
";
        var response = await resend.EmailSendAsync(new EmailMessage()
        {
            From = fromEmail!,
            To = email,
            Subject = "Confirm your Tripfinity account",
            HtmlBody =  htmlContent,
        });
    }
}