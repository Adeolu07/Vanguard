using _Tripfinity.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace _Tripfinity.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var apiKey = _config["SendGrid:Apikey"]; 
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("⚠️ SendGrid API key is null. Check appsettings.json or user-secrets.");
                throw new Exception("SendGrid API key not found in configuration");
            }

            Console.WriteLine("Loaded SendGrid API Key prefix: " + apiKey.Substring(0, 5));

            var client = new SendGridClient(apiKey);

            var fromEmail = _config["SendGrid:FromEmail"];
            var fromName = _config["SendGrid:FromName"];

            var from = new EmailAddress(fromEmail, fromName);
            var toAddress = new EmailAddress(to);

            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, body, body);

            //  Send and log response
            var response = await client.SendEmailAsync(msg);
            Console.WriteLine($"SendGrid Response: {response.StatusCode}");

            string responseBody = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"SendGrid Body: {responseBody}");
        }
    }
}
