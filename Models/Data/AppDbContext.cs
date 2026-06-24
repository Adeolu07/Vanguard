using _Tripfinity.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Models.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<BusTrip> BusTrips { get; set; }
    public DbSet<TaxiTrip> TaxiTrips { get; set; }
    public DbSet<RailwayTrip> RailwayTrips { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        builder.Entity<Booking>()
            .Property(booking => booking.TotalAmount)
            .HasPrecision(18, 2);

        builder.Entity<BusTrip>()
            .Property(b => b.Price)
            .HasPrecision(18, 2);

        builder.Entity<TaxiTrip>()
            .Property(t => t.Price)
            .HasPrecision(18, 2);

        builder.Entity<RailwayTrip>()
            .Property(r => r.Price)
            .HasPrecision(18, 2);

        builder.Entity<Booking>()
            .HasOne(b => b.BusTrip)
            .WithMany()
            .HasForeignKey(b => b.BusTripId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.TaxiTrip)
            .WithMany()
            .HasForeignKey(b => b.TaxiTripId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.RailwayTrip)
            .WithMany()
            .HasForeignKey(b => b.RailwayTripId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .Property(t => t.Fare)
            .HasPrecision(18, 2);

        builder.Entity<Ticket>()
            .HasIndex(t => t.QrToken)
            .IsUnique();

        builder.Entity<Ticket>()
            .HasOne(t => t.Booking)
            .WithMany()
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(18, 2);

        builder.Entity<WalletTransaction>()
            .Property(w => w.Amount)
            .HasPrecision(18, 2);

        builder.Entity<WalletTransaction>()
            .Property(w => w.BalanceAfter)
            .HasPrecision(18, 2);

        DataSeeding.Seed(builder);
    }
}