using _Tripfinity.Interfaces;
using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetBookingAsync(int id, string transportType)
    {
        IQueryable<Booking> query = _context.Bookings.Include(b => b.User);

        if (transportType == "Bus")
            query = query.Include(b => b.BusTrip);
        else if (transportType == "Railway")
            query = query.Include(b => b.RailwayTrip);
        else if (transportType == "Taxi")
            query = query.Include(b => b.TaxiTrip);

        return await query.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking?> BookBusAsync(int tripId, int seats, int? userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var trip = await _context.BusTrips.FindAsync(tripId);
        if (trip == null || user == null) return null;

        if (seats > trip.AvailableSeats) return null;
        trip.AvailableSeats -= seats;

        var booking = new Booking
        {
            UserId = user.Id,
            BusTripId = tripId,
            TransportType = "Bus",
            NumberOfSeats = seats,
            TotalAmount = trip.Price * seats,
            Status = "Confirmed",
            BookingDate = DateTime.Now
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> BookRailwayAsync(int tripId, int seats, string userEmail)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        var trip = await _context.RailwayTrips.FindAsync(tripId);
        if (trip == null || user == null) return null;

        if (seats > trip.AvailableSeats) return null;
        trip.AvailableSeats -= seats;

        var booking = new Booking
        {
            UserId = user.Id,
            RailwayTripId = tripId,
            TransportType = "Railway",
            NumberOfSeats = seats,
            TotalAmount = trip.Price * seats,
            Status = "Confirmed",
            BookingDate = DateTime.Now
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking?> BookTaxiAsync(int tripId, int seats, int? userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var trip = await _context.TaxiTrips.FindAsync(tripId);
        if (trip == null || user == null) return null;

        if (seats > trip.MaxPassengers) return null;

        var booking = new Booking
        {
            UserId = user.Id,
            TaxiTripId = tripId,
            TransportType = "Taxi",
            NumberOfSeats = seats,
            TotalAmount = trip.Price, 
            Status = "Confirmed",
            BookingDate = DateTime.Now
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<List<Booking>> GetRecentBookings(int id, string transportType)
    {
        // method to return all 5 bookings made recently

        var lastFiveBookings = await _context.Bookings
            .Where(user => user.Id == id)
            .Where(user => user.TransportType == transportType)
            .OrderByDescending(booking => booking.BookingDate)
            .Take(5)
            .ToListAsync();

        return lastFiveBookings;
        
    }
}
