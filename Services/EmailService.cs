using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using System.Net;
using System.Net.Mail;

namespace _Tripfinity.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");

        var host = smtpSettings["Host"] ?? "localhost";
        var portString = smtpSettings["Port"] ?? "25";
        var port = int.TryParse(portString, out var parsedPort) ? parsedPort : 25;

        var username = smtpSettings["Username"] ?? string.Empty;
        var password = smtpSettings["Password"] ?? string.Empty;
        var fromAddress = smtpSettings["From"] ?? "no-reply@tripfinity.com";

        var smtpClient = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromAddress),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await smtpClient.SendMailAsync(mailMessage);
    }

    public Task SendConfirmationEmailAsync(User user, string link) =>
        SendEmailAsync(user.Email, "Confirm your Tripfinity account",
            $"Click here to confirm your email: {link}");

    public Task SendResetEmailAsync(string email, string link) =>
        SendEmailAsync(email, "Reset your Tripfinity password",
            $"Click here to reset your password: {link}");
}
