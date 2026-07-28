using _Tripfinity.Models.Tables;

namespace _Tripfinity.Interfaces;

public interface IMarshalService
{
    Task<User?> GetMarshalAsync(int userId);
    Task<object?> GetMarshalTripsAsync(int marshalId, string vehicleType);
}