using _Tripfinity.Interfaces;
using Resend;

namespace _Tripfinity.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    public EmailService(IConfiguration config,  ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }
    

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var apiKey = _config["Resend:apiKey"];
            var fromEmail = _config["Resend:fromEmail"];

            IResend resend = ResendClient.Create(apiKey!);
            var response = await resend.EmailSendAsync(new EmailMessage
            {
                From = fromEmail!,
                To = email,
                Subject = subject,
                HtmlBody = htmlMessage,
            });

            if (!response.Success)
            {
                _logger.LogInformation("Email sent successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    
        
    public Task SendConfirmationEmailAsync(string email, string confirmationLink)
    {
        try
        {
            var htmlMessage = $@"
            <h2>Welcome to Tripfinity!</h2>
            <p>Please confirm your email by clicking the link below:</p>
            <a href='{confirmationLink}'>Confirm Email</a>
            <p>This link will expire in 24 hours.</p>";

            return SendEmailAsync(email, "Confirm Your Email on Tripfinity", htmlMessage);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    
    
    
}