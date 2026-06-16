// using _Tripfinity.Interfaces;
// using _Tripfinity.Models;
// using _Tripfinity.Models.Data;
//
// namespace _Tripfinity.Services;
//
// public class BookingService : IBookingService
// {
//     private readonly AppDbContext _context;
//     public BookingService(AppDbContext context)
//     {
//         _context = context;
//     }
//     
//     public async Task<Booking?> GetBookingAsync(int id)
//     {
//         var booking = await _context.Bookings.FindAsync(id);
//         if (booking == null) 
//             return null;
//         return booking;
//     }
//
//     public async Task<bool> BookTrainAsync(Booking booking)
//     {
//         if(HttpContext.Session.GetString("UserEmail") == null)
//             return false;
//         
//     }
//     
//     public async Task<bool> BookBusAsync(Booking booking)
//     {
//         return true;
//     }
//     
//     public async Task<bool> BookTaxiAsync(Booking booking)
//     {
//         return true;
//     }
//     
//     
//     
// }