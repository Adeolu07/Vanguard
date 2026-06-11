using _Tripfinity.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace _Tripfinity.Interfaces;

public interface IBookingService
{
    Task<bool> BookBusAsync(Booking booking);
    Task<bool> BookTrainAsync(Booking booking);
    Task<bool> BookTaxiAsync(Booking booking);
    
    
}