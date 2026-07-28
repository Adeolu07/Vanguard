using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class MarshalService : IMarshalService
{
    private readonly AppDbContext _context;

    public MarshalService(AppDbContext context) => _context = context;

    public async Task<User?> GetMarshalAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user is { Role: "Marshal" } ? user : null;
    }

    public async Task<object?> GetMarshalTripsAsync(int marshalId, string vehicleType)
    {
        return vehicleType switch
        {
            "Bus" => await _context.BusTrips
                .Where(t => t.MarshalId == marshalId)
                .OrderByDescending(t => t.DepartureTime)
                .ToListAsync(),
            "Railway" => await _context.RailwayTrips
                .Where(t => t.MarshalId == marshalId)
                .OrderByDescending(t => t.DepartureTime)
                .ToListAsync(),
            "Taxi" => await _context.TaxiTrips
                .Where(t => t.MarshalId == marshalId)
                .OrderByDescending(t => t.PickupTime)
                .ToListAsync(),
            _ => null
        };
    }
}