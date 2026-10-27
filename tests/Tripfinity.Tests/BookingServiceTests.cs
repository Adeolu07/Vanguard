using Tripfinity.Interfaces;
using Tripfinity.Models.Enums;
using Tripfinity.Models.Tables;
using Tripfinity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tripfinity.Tests;

public class BookingServiceTests
{
    private static BookingService CreateService(TestDatabase db, IPaymentService payments) =>
        new(db.Context, new TicketService(db.Context, NullLogger<TicketService>.Instance,
            TestDataProtection.CreatePayload(), TestDataProtection.Configuration()), payments,
            NullLogger<BookingService>.Instance);

    private static async Task<(int UserId, int TripId)> SeedTripAsync(TestDatabase db, int totalSeats)
    {
        var user = await db.AddUserAsync("rider@example.com", "CorrectHorse1");
        var trip = new BusTrip
        {
            From = "A", Destination = "B", Price = 500m,
            TotalSeats = totalSeats, AvailableSeats = totalSeats,
            DepartureTime = DateTime.Now.AddDays(2), VehicleId = "VEH-BUS-1",
            MarshalId = user.Id, CreatedAt = DateTime.Now
        };
        db.Context.BusTrips.Add(trip);
        await db.Context.SaveChangesAsync();
        return (user.Id, trip.Id);
    }

    private static Task<int> SeatsLeftAsync(TestDatabase db, int tripId) =>
        db.Context.BusTrips.Where(t => t.Id == tripId).Select(t => t.AvailableSeats).FirstAsync();

    [Fact]
    public async Task Successful_booking_claims_seats_and_issues_a_ticket()
    {
        using var db = new TestDatabase();
        var (userId, tripId) = await SeedTripAsync(db, totalSeats: 5);
        var service = CreateService(db, new FakePaymentService(succeed: true));

        var result = await service.BookAsync("bus", tripId, seats: 2, userId);

        Assert.True(result.Success);
        Assert.Equal(3, await SeatsLeftAsync(db, tripId));
        Assert.Equal(1, await db.Context.Tickets.CountAsync());
        Assert.Equal(1000m, result.Booking!.TotalAmount);
    }

    [Fact]
    public async Task Booking_more_seats_than_available_is_refused()
    {
        using var db = new TestDatabase();
        var (userId, tripId) = await SeedTripAsync(db, totalSeats: 2);
        var service = CreateService(db, new FakePaymentService(succeed: true));

        var result = await service.BookAsync("bus", tripId, seats: 3, userId);

        Assert.False(result.Success);
        Assert.Equal(2, await SeatsLeftAsync(db, tripId));
        Assert.Equal(0, await db.Context.Bookings.CountAsync());
    }

    [Fact]
    public async Task Repeated_bookings_cannot_oversell_the_trip()
    {
        using var db = new TestDatabase();
        var (userId, tripId) = await SeedTripAsync(db, totalSeats: 2);
        var service = CreateService(db, new FakePaymentService(succeed: true));

        var first = await service.BookAsync("bus", tripId, seats: 2, userId);
        var second = await service.BookAsync("bus", tripId, seats: 1, userId);

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Equal(0, await SeatsLeftAsync(db, tripId));
    }

    [Fact]
    public async Task Failed_payment_releases_the_seats_and_discards_the_booking()
    {
        using var db = new TestDatabase();
        var (userId, tripId) = await SeedTripAsync(db, totalSeats: 4);

        // SQLite connection is shared, so the raw seat update and the rollback both apply.
        var service = CreateService(db, new FakePaymentService(succeed: false));

        var result = await service.BookAsync("bus", tripId, seats: 3, userId);

        Assert.False(result.Success);
        Assert.Equal(4, await SeatsLeftAsync(db, tripId));
        Assert.Equal(0, await db.Context.Bookings.CountAsync());
        Assert.Equal(0, await db.Context.Tickets.CountAsync());
    }

    [Fact]
    public async Task Booking_without_a_wallet_is_refused_and_claims_no_seats()
    {
        using var db = new TestDatabase();
        var user = await db.AddUserAsync("nowallet@example.com", "CorrectHorse1", walletId: null);
        var trip = new BusTrip
        {
            From = "A", Destination = "B", Price = 500m, TotalSeats = 3, AvailableSeats = 3,
            DepartureTime = DateTime.Now.AddDays(1), VehicleId = "VEH-1", MarshalId = user.Id,
            CreatedAt = DateTime.Now
        };
        db.Context.BusTrips.Add(trip);
        await db.Context.SaveChangesAsync();

        var service = CreateService(db, new FakePaymentService(succeed: true));

        var result = await service.BookAsync("bus", trip.Id, seats: 1, user.Id);

        Assert.False(result.Success);
        Assert.Equal(3, await SeatsLeftAsync(db, trip.Id));
    }

    [Fact]
    public async Task Unknown_transport_type_is_refused()
    {
        using var db = new TestDatabase();
        var (userId, tripId) = await SeedTripAsync(db, totalSeats: 2);
        var service = CreateService(db, new FakePaymentService(succeed: true));

        var result = await service.BookAsync("hovercraft", tripId, seats: 1, userId);

        Assert.False(result.Success);
    }
}
