using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Services;
using _Tripfinity.Utility;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer
            (builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        
        builder.Services.AddControllersWithViews();
        builder.Services.AddControllers(); // Add API controllers

        var app = builder.Build();
        
        // using (var scope = app.Services.CreateScope())
        // {
        //     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //     dbContext.Database.EnsureCreated();
        // }
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseStaticFiles();
        app.UseSession();
        
        app.UseRouting();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Auth}/{action=SelectRole}/{id?}");
        
        app.MapControllers(); // For API controllers
        
        app.Run();
    }
}