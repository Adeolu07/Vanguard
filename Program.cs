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
        builder.Services.AddScoped<ITripListingService, TripListingService>();
        builder.Services.AddScoped<IMarshalService, MarshalService>();
        builder.Services.AddScoped<IPassengerService, PassengerService>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpClient<IWalletService, WalletService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["WalletStation:baseUrl"] ?? "");
        });
        builder.Services.AddMemoryCache();
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddControllersWithViews();
        builder.Services.AddControllers();
        var app = builder.Build();
        app.UseExceptionHandler();
        
        // using (var scope = app.Services.CreateScope())
        // {
        //     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //     dbContext.Database.EnsureCreated();
        // }
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