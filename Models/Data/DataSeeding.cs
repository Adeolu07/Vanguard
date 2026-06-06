using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Models.Data;

public static class DataSeeding
{
    public static void Seed(ModelBuilder builder)
    {
        builder.Entity<BusTrip>().HasData(
            new BusTrip
            {
                Id = 1,
                From = "Lagos",
                Destination = "Ibadan",
                DepartureTime = new DateTime(2026, 6, 8, 8, 0, 0),
                Price = 5000m,
                TotalSeats = 40,
                AvailableSeats = 40,
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6),
            },
            new BusTrip
            {
                Id = 2,
                From = "Lagos",
                Destination = "Jigawa",
                DepartureTime = new DateTime(2026, 6, 10, 8, 0, 0),
                Price = 5000m,
                TotalSeats = 40,
                AvailableSeats = 40,
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6),
            }
        );
    }
}