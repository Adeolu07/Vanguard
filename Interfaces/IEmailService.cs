namespace _Tripfinity.Interfaces;

public interface IEmailService 
{
    Task SendConfirmationEmailAsync(string email, string confirmationLink);
    Task SendPasswordResetEmailAsync(string email, string resetLink);
    public Task SendEmailAsync(string email, string subject, string htmlMessage);
}