using System.Net.Http.Headers;
using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Services;
using _Tripfinity.Utilities;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<ITicketService, TicketService>();
        builder.Services.AddScoped<ITripService, TripService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IWalletService, WalletService>();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddControllersWithViews();
        builder.Services.AddControllers();

        var app = builder.Build();

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseStaticFiles();
        app.UseSession();
        app.UseRouting();

        app.MapControllers();
        app.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");

        app.MapFallback(async context =>
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync("wwwroot/404.html");
        });

        app.Run();
    }
}