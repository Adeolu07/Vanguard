using System.Threading.RateLimiting;
using Tripfinity.Interfaces;
using Tripfinity.Models.Data;
using Tripfinity.Services;
using Tripfinity.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
namespace Tripfinity;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<ITicketService, TicketService>();
        builder.Services.AddScoped<ITripService, TripService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<ITripListingService, TripListingService>();
        builder.Services.AddScoped<IMarshalService, MarshalService>();
        builder.Services.AddScoped<IPassengerService, PassengerService>();
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<AdminOnlyFilter>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<ExternalTokenStore>();
        builder.Services.AddSingleton<TicketQrPayload>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Data Protection signs ticket QR payloads and encrypts partner tokens.
        // Keys must outlive the process (and be shared across instances) or every
        // restart invalidates previously issued QR codes.
        var keysPath = builder.Configuration["DataProtection:KeysPath"];
        var dataProtection = builder.Services.AddDataProtection().SetApplicationName("Tripfinity");
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            Directory.CreateDirectory(keysPath);
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        }
        builder.Services.AddHttpClient<IWalletService, WalletService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["WalletStation:BaseUrl"] ?? "");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Services.AddHttpClient<ICipService, CipService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["CipService:BaseUrl"] ?? "");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Services.AddHttpClient<IFastChannelService, FastChannelService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["FastChannel:BaseUrl"] ?? "");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        
        builder.Services.AddMemoryCache();
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.Name = ".Tripfinity.Session";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        builder.Services.AddAntiforgery(options =>
        {
            options.Cookie.Name = ".Tripfinity.Antiforgery";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RateLimitPolicies.Authentication, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
            options.AddPolicy(RateLimitPolicies.Sensitive, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        builder.Services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
        });

        builder.Services.AddControllersWithViews(options =>
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
        var app = builder.Build();
        app.UseExceptionHandler();


        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // Baseline security headers for every response (including static files).
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "img-src 'self' data:; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "script-src 'self' 'unsafe-inline'; " +
                "connect-src 'self'; " +
                "form-action 'self'; " +
                "frame-ancestors 'none'; " +
                "base-uri 'self'; " +
                "object-src 'none'";
            await next();
        });

        app.UseStaticFiles();
        app.UseSession();
        app.UseRouting();
        app.UseRateLimiter();
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

        if (builder.Configuration.GetValue<bool>("Seed:Enabled"))
        {
            await using var scope = app.Services.CreateAsyncScope();
            try
            {
                var seeder = new DataSeeder(
                    scope.ServiceProvider.GetRequiredService<AppDbContext>(),
                    scope.ServiceProvider.GetRequiredService<IWalletService>(),
                    scope.ServiceProvider.GetRequiredService<IConfiguration>(),
                    scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>());
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Admin seeding failed; continuing startup");
            }
        }

        await app.RunAsync();
    }
}