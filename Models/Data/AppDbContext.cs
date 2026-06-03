using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Models.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        :base(options)
    {}
    public DbSet<User> Users { get; set; }
    public DbSet<BusTrip> BusTrips { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    
        builder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();
        
        builder.Entity<Booking>()
            .Property(booking=> booking.Amount)
            .HasPrecision(18, 2);
    
        builder.Entity<BusTrip>()
            .Property(booking=> booking.Price)
            .HasPrecision(18, 2);
    
        builder.Entity<Booking>()
            .HasOne(booking => booking.User)
            .WithMany()
            .HasForeignKey(booking => booking.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    
        builder.Entity<Booking>()
            .HasOne(booking=> booking.BusTrip)
            .WithMany()
            .HasForeignKey(booking=> booking.BusTripId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}