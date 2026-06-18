using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace _Tripfinity.Interfaces;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string email, string confirmationLink);
}