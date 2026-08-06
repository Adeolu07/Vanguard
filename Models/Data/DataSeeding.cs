// ... existing code ...
using _Tripfinity.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Models.Data;

public static class DataSeeding
{
    public static void Seed(ModelBuilder builder)
    {
        var now = new DateTime(2026, 7, 5);

        // ──────────────────────────────────────────
        // MARSHAL USERS (for FK relationships)
        // ──────────────────────────────────────────
        var marshals = new List<User>
        {
            new()
            {
                Id = 101, // fixed PK for bus marshal
                Email = "busmarshal@tripfinity.com",
                PasswordHash = "Welcome123",
                FirstName = "Bus",
                LastName = "Marshal",
                PhoneNumber = "08010000001",
                Role = "Marshal",
                IsActive = true,
                IsEmailConfirmed = true,
                VehicleType = "Bus",
                LicenseId = "BUS-LIC-001",
                VehicleId = "VEH-BUS-A1B2C3D4",
                UserWalletId = "WALLET-BUS-M001"
            },
            new()
            {
                Id = 102, // railway marshal
                Email = "railwaymarshal@tripfinity.com",
                PasswordHash = "Welcome123",
                FirstName = "Railway",
                LastName = "Marshal",
                PhoneNumber = "08010000002",
                Role = "Marshal",
                IsActive = true,
                IsEmailConfirmed = true,
                VehicleType = "Railway",
                LicenseId = "RAIL-LIC-001",
                VehicleId = "VEH-RAIL-X9Y8Z7W6",
                UserWalletId = "WALLET-RAIL-M001"
            },
            new()
            {
                Id = 103, // taxi marshal
                Email = "taximarshal@tripfinity.com",
                PasswordHash = "Welcome123",
                FirstName = "Taxi",
                LastName = "Marshal",
                PhoneNumber = "08010000003",
                Role = "Marshal",
                IsActive = true,
                IsEmailConfirmed = true,
                VehicleType = "Taxi",
                LicenseId = "TAXI-LIC-001",
                VehicleId = "VEH-TAXI-P5Q6R7S8",
                UserWalletId = "WALLET-TAXI-M001"
            }
        };
        builder.Entity<User>().HasData(marshals);

        // ──────────────────────────────────────────
        // BUS TRIPS (belong to bus marshal)
        // ──────────────────────────────────────────
        var busTrips = new List<BusTrip>
        {
            new()
            {
                Id = 1, From = "Lagos", Destination = "Ibadan",
                DepartureTime = now.AddDays(1).AddHours(8), Price = 4500m,
                TotalSeats = 45, AvailableSeats = 45, IsActive = true, CreatedAt = now,
                MarshalId = 101, VehicleId = "VEH-BUS-A1B2C3D4"
            },
            new()
            {
                Id = 2, From = "Lagos", Destination = "Benin",
                DepartureTime = now.AddDays(2).AddHours(7), Price = 8500m,
                TotalSeats = 50, AvailableSeats = 50, IsActive = true, CreatedAt = now,
                MarshalId = 101, VehicleId = "VEH-BUS-A1B2C3D4"
            },
            // ... add all other bus trips with MarshalId = 101, VehicleId = "VEH-BUS-A1B2C3D4"
        };
        builder.Entity<BusTrip>().HasData(busTrips);

        // ──────────────────────────────────────────
        // RAILWAY TRIPS (belong to railway marshal)
        // ──────────────────────────────────────────
        var railwayTrips = new List<RailwayTrip>
        {
            new()
            {
                Id = 1, From = "Lagos", Destination = "Ibadan",
                Route = "Lagos–Ibadan", TrainClass = "Standard", Price = 3000m,
                TotalSeats = 280, AvailableSeats = 280,
                DepartureTime = now.AddDays(1).AddHours(8), IsActive = true, CreatedAt = now,
                MarshalId = 102, VehicleId = "VEH-RAIL-X9Y8Z7W6"
            },
            // ... add all other railway trips with MarshalId = 102, VehicleId = "VEH-RAIL-X9Y8Z7W6"
        };
        builder.Entity<RailwayTrip>().HasData(railwayTrips);

        // ──────────────────────────────────────────
        // TAXI TRIPS (belong to taxi marshal)
        // ──────────────────────────────────────────
        var taxiTrips = new List<TaxiTrip>
        {
            new()
            {
                Id = 1, PickupLocation = "Ikeja", DropoffLocation = "Victoria Island",
                Price = 12000m, MaxPassengers = 4,
                PickupTime = now.AddDays(1).AddHours(9), IsActive = true, CreatedAt = now,
                MarshalId = 103, VehicleId = "VEH-TAXI-P5Q6R7S8"
            },
            // ... add all other taxi trips with MarshalId = 103, VehicleId = "VEH-TAXI-P5Q6R7S8"
        };
        builder.Entity<TaxiTrip>().HasData(taxiTrips);
    }
}