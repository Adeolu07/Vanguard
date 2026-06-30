using Microsoft.EntityFrameworkCore;
using _Tripfinity.Models.Tables;

namespace _Tripfinity.Models.Data;

public static class DataSeeding
{
    public static void Seed(ModelBuilder builder)
    {
        var now = new DateTime(2026, 7, 5);
        var busTrips = new List<BusTrip>
        {
            new() { Id = 1,  From = "Lagos", Destination = "Ibadan",        DepartureTime = now.AddDays(1).AddHours(8),  Price = 4500m,  TotalSeats = 45, AvailableSeats = 45, IsActive = true, CreatedAt = now },
            new() { Id = 2,  From = "Lagos", Destination = "Benin",          DepartureTime = now.AddDays(2).AddHours(7),  Price = 8500m,  TotalSeats = 50, AvailableSeats = 50, IsActive = true, CreatedAt = now },
            new() { Id = 3,  From = "Lagos", Destination = "Abuja",          DepartureTime = now.AddDays(3).AddHours(6),  Price = 12000m, TotalSeats = 48, AvailableSeats = 48, IsActive = true, CreatedAt = now },
            new() { Id = 4,  From = "Lagos", Destination = "Port Harcourt",  DepartureTime = now.AddDays(4).AddHours(9),  Price = 10500m, TotalSeats = 50, AvailableSeats = 50, IsActive = true, CreatedAt = now },
            new() { Id = 5,  From = "Lagos", Destination = "Kano",           DepartureTime = now.AddDays(5).AddHours(5),  Price = 15000m, TotalSeats = 40, AvailableSeats = 40, IsActive = true, CreatedAt = now },
            new() { Id = 6,  From = "Abuja", Destination = "Kaduna",         DepartureTime = now.AddDays(1).AddHours(10), Price = 3500m,  TotalSeats = 45, AvailableSeats = 45, IsActive = true, CreatedAt = now },
            new() { Id = 7,  From = "Abuja", Destination = "Jos",            DepartureTime = now.AddDays(3).AddHours(7),  Price = 4200m,  TotalSeats = 42, AvailableSeats = 42, IsActive = true, CreatedAt = now },
            new() { Id = 8,  From = "Abuja", Destination = "Enugu",          DepartureTime = now.AddDays(4).AddHours(8),  Price = 7500m,  TotalSeats = 50, AvailableSeats = 50, IsActive = true, CreatedAt = now },
            new() { Id = 9,  From = "Port Harcourt", Destination = "Aba",     DepartureTime = now.AddDays(2).AddHours(12), Price = 2800m,  TotalSeats = 30, AvailableSeats = 30, IsActive = true, CreatedAt = now },
            new() { Id = 10, From = "Port Harcourt", Destination = "Calabar", DepartureTime = now.AddDays(5).AddHours(6), Price = 5500m,  TotalSeats = 40, AvailableSeats = 40, IsActive = true, CreatedAt = now },
            new() { Id = 11, From = "Benin", Destination = "Asaba",          DepartureTime = now.AddDays(3).AddHours(9),  Price = 3200m,  TotalSeats = 35, AvailableSeats = 35, IsActive = true, CreatedAt = now },
            new() { Id = 12, From = "Kano", Destination = "Katsina",         DepartureTime = now.AddDays(4).AddHours(7),  Price = 3800m,  TotalSeats = 36, AvailableSeats = 36, IsActive = true, CreatedAt = now },
            new() { Id = 13, From = "Ibadan", Destination = "Lagos",         DepartureTime = now.AddDays(1).AddHours(14), Price = 4500m,  TotalSeats = 45, AvailableSeats = 45, IsActive = true, CreatedAt = now },
            new() { Id = 14, From = "Ibadan", Destination = "Owerri",        DepartureTime = now.AddDays(5).AddHours(8),  Price = 6800m,  TotalSeats = 40, AvailableSeats = 40, IsActive = true, CreatedAt = now },
            new() { Id = 15, From = "Jos", Destination = "Abuja",            DepartureTime = now.AddDays(3).AddHours(13), Price = 4200m,  TotalSeats = 42, AvailableSeats = 42, IsActive = true, CreatedAt = now },
        };
        builder.Entity<BusTrip>().HasData(busTrips);

        // ──────────────────────────────────────────
        // RAILWAY TRIPS (modern Nigerian railway)
        // ──────────────────────────────────────────
        var railwayTrips = new List<RailwayTrip>
        {
            new() { Id = 1,  From = "Lagos",     Destination = "Ibadan",     Route = "Lagos–Ibadan",      TrainClass = "Standard", Price = 3000m,  TotalSeats = 280, AvailableSeats = 280, DepartureTime = now.AddDays(1).AddHours(8),  IsActive = true, CreatedAt = now },
            new() { Id = 2,  From = "Ibadan",    Destination = "Lagos",      Route = "Lagos–Ibadan",      TrainClass = "Standard", Price = 3000m,  TotalSeats = 280, AvailableSeats = 280, DepartureTime = now.AddDays(1).AddHours(15), IsActive = true, CreatedAt = now },
            new() { Id = 3,  From = "Abuja",     Destination = "Kaduna",     Route = "Abuja–Kaduna",      TrainClass = "First",   Price = 9000m,  TotalSeats = 100, AvailableSeats = 100, DepartureTime = now.AddDays(2).AddHours(7),  IsActive = true, CreatedAt = now },
            new() { Id = 4,  From = "Kaduna",    Destination = "Abuja",      Route = "Abuja–Kaduna",      TrainClass = "First",   Price = 9000m,  TotalSeats = 100, AvailableSeats = 100, DepartureTime = now.AddDays(2).AddHours(14), IsActive = true, CreatedAt = now },
            new() { Id = 5,  From = "Warri",     Destination = "Itakpe",     Route = "Warri–Itakpe",      TrainClass = "Standard", Price = 2500m,  TotalSeats = 240, AvailableSeats = 240, DepartureTime = now.AddDays(3).AddHours(9),  IsActive = true, CreatedAt = now },
            new() { Id = 6,  From = "Itakpe",    Destination = "Warri",      Route = "Warri–Itakpe",      TrainClass = "Standard", Price = 2500m,  TotalSeats = 240, AvailableSeats = 240, DepartureTime = now.AddDays(3).AddHours(16), IsActive = true, CreatedAt = now },
            new() { Id = 7,  From = "Lagos",     Destination = "Ibadan",     Route = "Lagos–Ibadan",      TrainClass = "Business", Price = 5800m,  TotalSeats = 80,  AvailableSeats = 80,  DepartureTime = now.AddDays(2).AddHours(7),  IsActive = true, CreatedAt = now },
            new() { Id = 8,  From = "Abuja",     Destination = "Kaduna",     Route = "Abuja–Kaduna",      TrainClass = "Business", Price = 6500m,  TotalSeats = 95,  AvailableSeats = 95,  DepartureTime = now.AddDays(3).AddHours(11), IsActive = true, CreatedAt = now },
            new() { Id = 9,  From = "Port Harcourt", Destination = "Aba",     Route = "Port Harcourt–Aba", TrainClass = "Standard", Price = 2000m,  TotalSeats = 160, AvailableSeats = 160, DepartureTime = now.AddDays(4).AddHours(8),  IsActive = true, CreatedAt = now },
            new() { Id = 10, From = "Aba",       Destination = "Port Harcourt", Route = "Port Harcourt–Aba", TrainClass = "Standard", Price = 2000m,  TotalSeats = 160, AvailableSeats = 160, DepartureTime = now.AddDays(4).AddHours(15), IsActive = true, CreatedAt = now },
            new() { Id = 11, From = "Lagos",     Destination = "Abeokuta",   Route = "Lagos–Abeokuta",    TrainClass = "Standard", Price = 1500m,  TotalSeats = 180, AvailableSeats = 180, DepartureTime = now.AddDays(5).AddHours(7),  IsActive = true, CreatedAt = now },
            new() { Id = 12, From = "Abeokuta",  Destination = "Lagos",      Route = "Lagos–Abeokuta",    TrainClass = "Standard", Price = 1500m,  TotalSeats = 180, AvailableSeats = 180, DepartureTime = now.AddDays(5).AddHours(16), IsActive = true, CreatedAt = now },
            new() { Id = 13, From = "Abuja",     Destination = "Minna",      Route = "Abuja–Minna",       TrainClass = "Standard", Price = 2800m,  TotalSeats = 200, AvailableSeats = 200, DepartureTime = now.AddDays(6).AddHours(8),  IsActive = true, CreatedAt = now },
            new() { Id = 14, From = "Minna",     Destination = "Abuja",      Route = "Abuja–Minna",       TrainClass = "Standard", Price = 2800m,  TotalSeats = 200, AvailableSeats = 200, DepartureTime = now.AddDays(6).AddHours(15), IsActive = true, CreatedAt = now },
            new() { Id = 15, From = "Lagos",     Destination = "Ibadan",     Route = "Lagos–Ibadan",      TrainClass = "First",   Price = 7500m,  TotalSeats = 48,  AvailableSeats = 48,  DepartureTime = now.AddDays(3).AddHours(9),  IsActive = true, CreatedAt = now },
        };
        builder.Entity<RailwayTrip>().HasData(railwayTrips);

        // ──────────────────────────────────────────
        // TAXI TRIPS (intra‑city / short haul)
        // ──────────────────────────────────────────
        var taxiTrips = new List<TaxiTrip>
        {
            new() { Id = 1,  PickupLocation = "Ikeja",        DropoffLocation = "Victoria Island", Price = 12000m, MaxPassengers = 4, PickupTime = now.AddDays(1).AddHours(9),  IsActive = true, CreatedAt = now },
            new() { Id = 2,  PickupLocation = "Ikeja",        DropoffLocation = "Lekki",          Price = 10000m, MaxPassengers = 4, PickupTime = now.AddDays(1).AddHours(14), IsActive = true, CreatedAt = now },
            new() { Id = 3,  PickupLocation = "Lekki",        DropoffLocation = "Ikeja",          Price = 10000m, MaxPassengers = 4, PickupTime = now.AddDays(2).AddHours(8),  IsActive = true, CreatedAt = now },
            new() { Id = 4,  PickupLocation = "Victoria Island", DropoffLocation = "Ikeja",       Price = 12000m, MaxPassengers = 4, PickupTime = now.AddDays(2).AddHours(15), IsActive = true, CreatedAt = now },
            new() { Id = 5,  PickupLocation = "Abuja CBD",    DropoffLocation = "Garki",          Price = 4500m,  MaxPassengers = 4, PickupTime = now.AddDays(3).AddHours(10), IsActive = true, CreatedAt = now },
            new() { Id = 6,  PickupLocation = "Garki",        DropoffLocation = "Wuse",           Price = 3500m,  MaxPassengers = 4, PickupTime = now.AddDays(3).AddHours(16), IsActive = true, CreatedAt = now },
            new() { Id = 7,  PickupLocation = "Wuse",         DropoffLocation = "Maitama",        Price = 4000m,  MaxPassengers = 4, PickupTime = now.AddDays(4).AddHours(8),  IsActive = true, CreatedAt = now },
            new() { Id = 8,  PickupLocation = "Port Harcourt",DropoffLocation = "Rumuokoro",      Price = 5000m,  MaxPassengers = 4, PickupTime = now.AddDays(4).AddHours(13), IsActive = true, CreatedAt = now },
            new() { Id = 9,  PickupLocation = "Kano",         DropoffLocation = "Sabon Gari",     Price = 3000m,  MaxPassengers = 4, PickupTime = now.AddDays(5).AddHours(11), IsActive = true, CreatedAt = now },
            new() { Id = 10, PickupLocation = "Ibadan",       DropoffLocation = "Mokola",         Price = 3500m,  MaxPassengers = 4, PickupTime = now.AddDays(5).AddHours(17), IsActive = true, CreatedAt = now },
            new() { Id = 11, PickupLocation = "Benin",        DropoffLocation = "Ugbowo",         Price = 4000m,  MaxPassengers = 4, PickupTime = now.AddDays(6).AddHours(8),  IsActive = true, CreatedAt = now },
            new() { Id = 12, PickupLocation = "Enugu",        DropoffLocation = "New Haven",      Price = 3800m,  MaxPassengers = 4, PickupTime = now.AddDays(6).AddHours(14), IsActive = true, CreatedAt = now },
        };
        builder.Entity<TaxiTrip>().HasData(taxiTrips);
    }
}