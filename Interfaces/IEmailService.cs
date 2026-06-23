namespace _Tripfinity.Interfaces;

public interface IEmailService 
{
    Task SendConfirmationEmailAsync(string email, string confirmationLink);
    public Task SendEmailAsync(string email, string subject, string htmlMessage);
}