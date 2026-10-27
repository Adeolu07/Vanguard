using Tripfinity.Models.Enums;
using Tripfinity.Models.Tables;
using Tripfinity.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tripfinity.Tests;

public class TicketServiceTests
{
    private const string Vehicle = "VEH-BUS-1";

    private static async Task<(TestDatabase Db, Ticket Ticket)> SeedTicketAsync(TicketStatus status,
        BookingStatus bookingStatus = BookingStatus.Confirmed)
    {
        var db = new TestDatabase();
        var user = await db.AddUserAsync("p@example.com", "CorrectHorse1");

        var trip = new BusTrip
        {
            From = "A", Destination = "B", Price = 1000m, TotalSeats = 10, AvailableSeats = 9,
            DepartureTime = DateTime.Now.AddDays(1), VehicleId = Vehicle, MarshalId = user.Id,
            CreatedAt = DateTime.Now
        };
        db.Context.BusTrips.Add(trip);
        await db.Context.SaveChangesAsync();

        var booking = new Booking
        {
            UserId = user.Id, BusTripId = trip.Id, TransportType = TransportType.Bus,
            NumberOfSeats = 1, TotalAmount = 1000m, Status = bookingStatus,
            BookingDate = DateTime.Now, PaymentTransactionId = "txn-1"
        };
        db.Context.Bookings.Add(booking);
        await db.Context.SaveChangesAsync();

        var ticket = new Ticket
        {
            TicketReference = "TKT-ABCD1234",
            BookingId = booking.Id,
            PassengerId = user.Id,
            VehicleId = Vehicle,
            TransportType = TransportType.Bus,
            TripTime = trip.DepartureTime,
            Fare = 1000m,
            Status = status,
            IssuedAt = DateTime.Now
        };
        db.Context.Tickets.Add(ticket);
        await db.Context.SaveChangesAsync();

        return (db, ticket);
    }

    private static TicketService CreateService(TestDatabase db, bool acceptLegacyUnsignedQr = false) =>
        new(db.Context, NullLogger<TicketService>.Instance, TestDataProtection.CreatePayload(),
            TestDataProtection.Configuration(acceptLegacyUnsignedQr));

    private static string Code(string ticketReference) =>
        TestDataProtection.CreatePayload().Protect(ticketReference);

    [Fact]
    public void A_ticket_code_is_not_the_bare_reference()
    {
        Assert.NotEqual("TKT-ABCD1234", Code("TKT-ABCD1234"));
        Assert.StartsWith("TKT1.", Code("TKT-ABCD1234"));
    }

    [Fact]
    public void Generated_qr_code_is_a_png()
    {
        var service = CreateServiceForQr();

        var png = service.GenerateQrCode("TKT-ABCD1234");

        Assert.True(png.Length > 100);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, png[..4]);
    }

    private static TicketService CreateServiceForQr()
    {
        var db = new TestDatabase();
        return new TicketService(db.Context, NullLogger<TicketService>.Instance,
            TestDataProtection.CreatePayload(), TestDataProtection.Configuration());
    }

    [Fact]
    public async Task Unsigned_reference_is_rejected()
    {
        var (db, ticket) = await SeedTicketAsync(TicketStatus.Issued);
        using var _ = db;
        var service = CreateService(db);

        // A guesser who knows the reference format must not be able to build a code.
        var result = await service.ValidateTicketAsync(ticket.TicketReference, marshalId: 42, Vehicle);

        Assert.False(result.Success);
        Assert.Contains("not valid", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(TicketStatus.Issued, db.Context.Tickets.Single().Status);
    }

    [Fact]
    public async Task Tampered_code_is_rejected()
    {
        var (db, ticket) = await SeedTicketAsync(TicketStatus.Issued);
        using var _ = db;
        var service = CreateService(db);

        var tampered = Code(ticket.TicketReference);
        tampered = tampered[..^2] + (tampered[^2] == 'A' ? "BB" : "AA");

        var result = await service.ValidateTicketAsync(tampered, marshalId: 42, Vehicle);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Legacy_unsigned_reference_is_only_accepted_when_explicitly_enabled()
    {
        var (db, ticket) = await SeedTicketAsync(TicketStatus.Issued);
        using var _ = db;
        var service = CreateService(db, acceptLegacyUnsignedQr: true);

        var result = await service.ValidateTicketAsync(ticket.TicketReference, marshalId: 42, Vehicle);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task Issued_ticket_is_accepted_once()
    {
        var (db, ticket) = await SeedTicketAsync(TicketStatus.Issued);
        using var _ = db;
        var service = CreateService(db);

        var first = await service.ValidateTicketAsync(Code(ticket.TicketReference), marshalId: 42, Vehicle);
        var second = await service.ValidateTicketAsync(Code(ticket.TicketReference), marshalId: 42, Vehicle);

        Assert.True(first.Success);
        Assert.Equal(42, first.Ticket!.ValidatedByMarshalId);
        Assert.False(second.Success);
        Assert.Contains("already validated", second.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(TicketStatus.Expired)]
    [InlineData(TicketStatus.Cancelled)]
    [InlineData(TicketStatus.Validated)]
    public async Task Tickets_that_are_not_freshly_issued_are_rejected(TicketStatus status)
    {
        var (db, ticket) = await SeedTicketAsync(status);
        using var _ = db;
        var service = CreateService(db);

        var result = await service.ValidateTicketAsync(Code(ticket.TicketReference), marshalId: 42, Vehicle);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Ticket_can_only_be_validated_by_its_own_vehicle()
    {
        var (db, ticket) = await SeedTicketAsync(TicketStatus.Issued);
        using var _ = db;
        var service = CreateService(db);

        var result = await service.ValidateTicketAsync(Code(ticket.TicketReference), marshalId: 42, "VEH-OTHER-9");

        Assert.False(result.Success);
        Assert.Contains("different vehicle", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Ticket_without_a_confirmed_booking_is_rejected()
    {
        var (db, ticket) = await SeedTicketAsync(TicketStatus.Issued, BookingStatus.Cancelled);
        using var _ = db;
        var service = CreateService(db);

        var result = await service.ValidateTicketAsync(Code(ticket.TicketReference), marshalId: 42, Vehicle);

        Assert.False(result.Success);
        Assert.Contains("confirmed booking", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unknown_reference_is_rejected()
    {
        var (db, _) = await SeedTicketAsync(TicketStatus.Issued);
        using var _unused = db;
        var service = CreateService(db);

        var result = await service.ValidateTicketAsync(Code("TKT-NOT-REAL"), marshalId: 42, Vehicle);

        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
