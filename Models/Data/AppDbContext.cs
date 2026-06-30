using _Tripfinity.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Models.Data
{
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
        public DbSet<AuthToken> AuthTokens { get; set; }

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
                .Property(bus => bus.Price)
                .HasPrecision(18, 2);

            builder.Entity<TaxiTrip>()
                .Property(taxi => taxi.Price)
                .HasPrecision(18, 2);

            builder.Entity<RailwayTrip>()
                .Property(train => train.Price)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .HasOne(booking => booking.BusTrip)
                .WithMany()
                .HasForeignKey(booking => booking.BusTripId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(booking => booking.TaxiTrip)
                .WithMany()
                .HasForeignKey(booking => booking.TaxiTripId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(booking => booking.RailwayTrip)
                .WithMany()
                .HasForeignKey(booking => booking.RailwayTripId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Ticket>()
                .Property(ticket => ticket.Fare)
                .HasPrecision(18, 2);

            //  Removed QrToken index, replaced with TicketReference
            builder.Entity<Ticket>()
                .HasIndex(ticket => ticket.TicketReference)
                .IsUnique();

            builder.Entity<Ticket>()
                .HasOne(ticket => ticket.Booking)
                .WithMany()
                .HasForeignKey(ticket => ticket.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AuthToken>()
                .Property(token => token.Token);

            DataSeeding.Seed(builder);
        }
    }
}
