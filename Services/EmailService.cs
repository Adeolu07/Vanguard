using _Tripfinity.Interfaces;
using System.Net.Mail;
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
            const string subject = "Confirm your email on Tripfinity";
            var htmlBody = $$$"""
                              <!DOCTYPE html>
                              <html>
                              <head>
                                  <style>
                                      body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f8fafc; margin:0; padding:0; }}
                                      .container {{ max-width: 480px; margin: 40px auto; background: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); overflow: hidden; }}
                                      .header {{ background: #0f254c; padding: 32px 24px; text-align: center; }}
                                      .header h1 {{ color: #ffffff; font-size: 22px; margin:0; font-weight: 700; }}
                                      .body {{ padding: 32px 24px; text-align: center; }}
                                      .body h2 {{ color: #0f172a; font-size: 20px; margin-bottom: 12px; }}
                                      .body p {{ color: #64748b; font-size: 14px; line-height: 1.5; margin: 16px 0; }}
                                      .btn {{ display: inline-block; background: #0f254c; color: #ffffff; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 14px; margin-top: 8px; }}
                                      .footer {{ padding: 16px 24px; background: #f8fafc; text-align: center; font-size: 11px; color: #a0aec0; }}
                                      .footer a {{ color: #64748b; text-decoration: none; }}
                                  </style>
                              </head>
                              <body>
                                  <div class='container'>
                                      <div class='header'>
                                          <h1>Tripfinity</h1>
                                      </div>
                                      <div class='body'>
                                          <h2>Welcome to Tripfinity!</h2>
                                          <p>Please confirm your email address by clicking the button below. This link is valid for <strong>24 hours</strong>.</p>
                                          <a class='btn' href='{confirmationLink}'>Confirm My Email</a>
                                          <p style='font-size:12px; color:#94a3b8;'>If you didn’t create an account, you can safely ignore this email.</p>
                                      </div>
                                      <div class='footer'>
                                          Tripfinity Ecosystem &copy; {DateTime.Now.Year} &middot; <a href='#'>Help</a>
                                      </div>
                                  </div>
                              </body>
                              </html>
                              """;

            return SendEmailAsync(email, subject, htmlBody);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    
    public async Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        const string subject = "Reset your Tripfinity password";
        var htmlBody = $$"""
                         <!DOCTYPE html>
                         <html>

                             <style>
                                 body { font-family: 'Segoe UI', Arial, sans-serif; background-color: #f8fafc; margin:0; padding:0; }
                                 .container { max-width: 480px; margin: 40px auto; background: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.05); overflow: hidden; }
                                 .header { background: #0f254c; padding: 32px 24px; text-align: center; }
                                 .header h1 { color: #ffffff; font-size: 22px; margin:0; font-weight: 700; }
                                 .body { padding: 32px 24px; text-align: center; }
                                 .body h2 { color: #0f172a; font-size: 20px; margin-bottom: 12px; }
                                 .body p { color: #64748b; font-size: 14px; line-height: 1.5; margin: 16px 0; }
                                 .btn { display: inline-block; background: #0f254c; color: #ffffff; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 14px; margin-top: 8px; }
                                 .footer { padding: 16px 24px; background: #f8fafc; text-align: center; font-size: 11px; color: #a0aec0; }
                                 .footer a { color: #64748b; text-decoration: none; }
                             </style>
                         </head>
                         <body>
                             <div class='container'>
                                 <div class='header'>
                                     <h1>Tripfinity</h1>
                                 </div>
                                 <div class='body'>
                                     <h2>Forgot your password?</h2>
                                     <p>Tap the button below to reset your password. This link is valid for <strong>1 hour</strong>.</p>
                                     <a class='btn' href='{{resetLink}}'>Reset My Password</a>
                                     <p style='font-size:12px; color:#94a3b8;'>If you didn’t request this, please ignore this email.</p>
                                 </div>
                                 <div class='footer'>
                                     Tripfinity Ecosystem &copy; {{DateTime.Now.Year}} &middot; <a href='#'>Help</a>
                                 </div>
                             </div>
                         </body>
                         </html>
                         """;

        await SendEmailAsync(email, subject, htmlBody);
    }
    
    
    
}