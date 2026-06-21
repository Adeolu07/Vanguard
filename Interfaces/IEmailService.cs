namespace _Tripfinity.Interfaces;

public interface IEmailService 
{
    Task SendConfirmationEmailAsync(string email, string confirmationLink);
}