using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Services;
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

        var app = builder.Build();
        app.UseStaticFiles();
        app.UseSession();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/");
        
        app.Run();
    }
}