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
        public DbSet<Transaction> Transactions { get; set; }
        
        public DbSet<ExternalApiToken> ExternalApiTokens { get; set; }
        public DbSet<MarshalBankAccount> MarshalBankAccounts { get; set; }

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
                .HasOne<User>()
                .WithMany(u=>u.BusTrips)
                .HasForeignKey(t => t.MarshalId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<TaxiTrip>()
                .HasOne<User>()
                .WithMany(u=> u.TaxiTrips)
                .HasForeignKey(t => t.MarshalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RailwayTrip>()
                .HasOne<User>()
                .WithMany(u=>u.RailwayTrips)
                .HasForeignKey(t => t.MarshalId)
                .OnDelete(DeleteBehavior.Restrict);

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

            builder.Entity<Transaction>()
                .HasOne(transaction => transaction.User)
                .WithMany(user => user.Transactions)
                .HasForeignKey(transaction => transaction.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .Property(transaction => transaction.Amount)
                .HasPrecision(18, 2);
            
            builder.Entity<MarshalBankAccount>()
                .HasKey(account => account.MarshalId);

            builder.Entity<MarshalBankAccount>()
                .HasOne(account => account.Marshal)
                .WithOne()
                .HasForeignKey<MarshalBankAccount>(account => account.MarshalId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<ExternalApiToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Provider).IsUnique();
                entity.Property(e => e.Provider).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Token).IsRequired();
            });
        }
    }
}
