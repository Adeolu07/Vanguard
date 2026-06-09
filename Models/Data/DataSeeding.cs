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

        builder.Entity<RailwayTrip>().HasData(
            new RailwayTrip
            {
                Id = 1,
                From = "Abuja",
                Destination = "Kaduna",
                Route = "AKTS",
                TrainClass = "First",
                Price = 9000m,
                TotalSeats = 100,
                AvailableSeats = 100,
                DepartureTime = new DateTime(2026, 6, 20, 7, 0, 0),
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6)
            },
            new RailwayTrip
            {
                Id = 2,
                From = "Abuja",
                Destination = "Kaduna",
                Route = "AKTS",
                TrainClass = "Business",
                Price = 6500m,
                TotalSeats = 150,
                AvailableSeats = 150,
                DepartureTime = new DateTime(2026, 6, 20, 7, 0, 0),
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6)
            },
            new RailwayTrip
            {
                Id = 3,
                From = "Abuja",
                Destination = "Kaduna",
                Route = "AKTS",
                TrainClass = "Regular",
                Price = 3600m,
                TotalSeats = 200,
                AvailableSeats = 200,
                DepartureTime = new DateTime(2026, 6, 20, 7, 0, 0),
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6)
            },
            new RailwayTrip
            {
                Id = 4,
                From = "Kaduna",
                Destination = "Abuja",
                Route = "AKTS",
                TrainClass = "Regular",
                Price = 3600m,
                TotalSeats = 200,
                AvailableSeats = 200,
                DepartureTime = new DateTime(2026, 6, 20, 14, 0, 0),
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6)
            },
            new RailwayTrip
            {
                Id = 5,
                From = "Lagos",
                Destination = "Ibadan",
                Route = "Lagos-Ibadan",
                TrainClass = "Regular",
                Price = 3000m,
                TotalSeats = 300,
                AvailableSeats = 300,
                DepartureTime = new DateTime(2026, 6, 21, 8, 0, 0),
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6)
            },
            new RailwayTrip
            {
                Id = 6,
                From = "Ibadan",
                Destination = "Lagos",
                Route = "Lagos-Ibadan",
                TrainClass = "Regular",
                Price = 3000m,
                TotalSeats = 300,
                AvailableSeats = 300,
                DepartureTime = new DateTime(2026, 6, 21, 15, 0, 0),
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6)
            },
            new RailwayTrip
            {
                Id = 7,
                From = "Warri",
                Destination = "Itakpe",
                Route = "Warri-Itakpe",
                TrainClass = "Regular",
                Price = 2500m,
                TotalSeats = 250,
                AvailableSeats = 250,
                DepartureTime = new DateTime(2026, 6, 22, 9, 0, 0),
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6)
            },
            new RailwayTrip
            {
                Id = 8,
                From = "Itakpe",
                Destination = "Warri",
                Route = "Warri-Itakpe",
                TrainClass = "Regular",
                Price = 2500m,
                TotalSeats = 250,
                AvailableSeats = 250,
                DepartureTime = new DateTime(2026, 6, 22, 16, 0, 0),
                IsActive = true,
                CreatedAt = new DateTime(2026, 6, 6)
            }
        );
    }
}