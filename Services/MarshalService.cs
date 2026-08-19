using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Data.Requests;
using _Tripfinity.Models.Data.Response;
using _Tripfinity.Models.Tables;
using _Tripfinity.Models.ViewModels;
using _Tripfinity.Utilities;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class MarshalService : IMarshalService
{
    private readonly AppDbContext _context;
    private readonly IWalletService _walletService;
    private readonly ICipService _cipService;

    public MarshalService(AppDbContext context, IWalletService walletService, ICipService cipService)
    {
     _context = context;
     _walletService = walletService;
     _cipService = cipService;
    }
        

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
    
    public async Task<string?> GetMarshalWalletIdAsync(Booking booking)
    {
        var marshalId = booking.BusTrip?.MarshalId
                        ?? booking.RailwayTrip?.MarshalId
                        ?? booking.TaxiTrip?.MarshalId;
        if (marshalId == null) return null;
        var marshal = await _context.Users.FindAsync(marshalId.Value);
        return marshal?.UserWalletId;
    }

    public async Task<MarshalWalletViewModel> GetWalletInfoAsync(int userId, int page = 1)
    {
        var marshal = await _context.Users.FindAsync(userId);
        if (marshal is not { Role: "Marshal" } || string.IsNullOrWhiteSpace(marshal.UserWalletId))
            return new MarshalWalletViewModel
            {
                WalletId = null,
                Balance = 0,
                Transactions = new List<TransactionDetailsList>(),
                CurrentPage = page,
                TotalPages = 1,
                HasNext = false,
                HasPrevious = false
            };

        return await _walletService.BuildWalletInfoAsync(marshal.UserWalletId, page);
    }

    public async Task<MarshalDashboardViewModel?> GetMarshalDashboardAsync(int marshalId)
    {
        var marshal = await GetMarshalAsync(marshalId);
        if (marshal == null) return null;

        var tickets = await _context.Tickets
            .Include(t => t.Booking)
            .ThenInclude(b => b.User)
            .Where(t => t.VehicleId == marshal.VehicleId)
            .OrderByDescending(t => t.TripTime)
            .Take(20)
            .ToListAsync();

        return new MarshalDashboardViewModel
        {
            MarshalId = marshal.Id,
            FirstName = marshal.FirstName,
            VehicleType = marshal.VehicleType ?? "",
            VehicleId = marshal.VehicleId ?? "",
            Tickets = tickets
        };
    }

    public async Task<MarshalBankAccount?> GetBankAccountAsync(int userId) =>
        await _context.MarshalBankAccounts.FindAsync(userId);

    public async Task<ServiceResult> AddBankAccountAsync(int userId, string accountNumber, string bankCode)
    {
        var marshal = await GetMarshalAsync(userId);
        if (marshal == null) return ServiceResult.Fail("Marshal not found.");

        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length != 10 || !accountNumber.All(char.IsDigit))
            return ServiceResult.Fail("Enter a valid 10-digit account number.");

        var bank = Banks.All.FirstOrDefault(b => b.Code == bankCode);
        if (bank == null) return ServiceResult.Fail("Select a valid bank.");
        
        var expectedName = $"{marshal.FirstName} {marshal.LastName}".Trim();

        // Payout account is the marshal's own account — use their legal name
        var enquiry = await _cipService.AccountEnquiry(accountNumber, bankCode);
        if (enquiry?.Data == null || enquiry.Data.ResponseCode != "00")
            return ServiceResult.Fail(enquiry?.Message ?? "Name enquiry failed. Please try again.");

        if (!NamesMatch(expectedName, enquiry.Data.AccountName))
            return ServiceResult.Fail("The account name does not match your registered name.");
        
        var existing = await _context.MarshalBankAccounts.FindAsync(userId);
        if (existing == null)
        {
            _context.MarshalBankAccounts.Add(new MarshalBankAccount
            {
                MarshalId = userId,
                AccountNumber = accountNumber,
                BankCode = bankCode,
                BankName = bank.Name,
                AccountName = enquiry.Data.AccountName
            });
        }
        else
        {
            existing.AccountNumber = accountNumber;
            existing.BankCode = bankCode;
            existing.BankName = bank.Name;
            existing.AccountName = enquiry.Data.AccountName;
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok("Bank account verified and saved.");
    }

    public async Task<ServiceResult> CashOutAsync(int userId, decimal amount)
    {
        var marshal = await GetMarshalAsync(userId);
        if (marshal == null || string.IsNullOrEmpty(marshal.UserWalletId))
            return ServiceResult.Fail("Wallet not available.");

        var bank = await _context.MarshalBankAccounts.FindAsync(userId);
        if (bank == null)
            return ServiceResult.Fail("No bank account linked. Add one from your profile.");

        if (amount <= 0)
            return ServiceResult.Fail("Enter a valid amount.");

        var debit = await _walletService.DebitWalletAsync(new DebitWalletRequest
        {
            Amount = amount,
            CustomerId = marshal.UserWalletId,
            Description = $"Cash out to {bank.AccountNumber} ({bank.BankName})",
            TraceId = Guid.NewGuid().ToString("N")
        });

        if (debit.ResponseHeader.ResponseCode != "00")
            return ServiceResult.Fail(debit.ResponseHeader.ResponseMessage ?? "Cash out failed.");

        return ServiceResult.Ok("Cash out initiated.");
    }

    public async Task<TripDetailViewModel?> GetTripDetailAsync(int tripId, int marshalId, string vehicleType)
    {
        object? trip;
        IEnumerable<Booking> bookings;

        switch (vehicleType.ToLower())
        {
            case "bus":
                trip = await _context.BusTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.MarshalId == marshalId);
                if (trip == null) return null;
                bookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.BusTrip)
                    .Where(b => b.BusTripId == tripId)
                    .ToListAsync();
                break;
            case "railway":
                trip = await _context.RailwayTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.MarshalId == marshalId);
                if (trip == null) return null;
                bookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.RailwayTrip)
                    .Where(b => b.RailwayTripId == tripId)
                    .ToListAsync();
                break;
            case "taxi":
                trip = await _context.TaxiTrips.FirstOrDefaultAsync(t => t.Id == tripId && t.MarshalId == marshalId);
                if (trip == null) return null;
                bookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.TaxiTrip)
                    .Where(b => b.TaxiTripId == tripId)
                    .ToListAsync();
                break;
            default:
                return null;
        }

        return new TripDetailViewModel
        {
            TripId = tripId,
            TransportType = vehicleType,
            Route = trip switch
            {
                BusTrip b => $"{b.From} → {b.Destination}",
                RailwayTrip r => $"{r.From} → {r.Destination}",
                TaxiTrip t => $"{t.PickupLocation} → {t.DropoffLocation}",
                _ => ""
            },
            DepartureTime = trip switch
            {
                BusTrip b => b.DepartureTime,
                RailwayTrip r => r.DepartureTime,
                TaxiTrip t => t.PickupTime,
                _ => DateTime.MinValue
            },
            Status = trip switch
            {
                BusTrip b => b.Status.ToString(),
                RailwayTrip r => r.Status.ToString(),
                TaxiTrip t => t.Status.ToString(),
                _ => ""
            },
            Passengers = bookings.Select(b => new TripPassenger
            {
                PassengerName = b.User != null ? $"{b.User.FirstName} {b.User.LastName}" : "Unknown",
                Seats = b.NumberOfSeats,
                BookingStatus = b.Status.ToString(),
                HasTicket = _context.Tickets.Any(t => t.BookingId == b.Id),
                TicketStatus = _context.Tickets
                    .Where(t => t.BookingId == b.Id)
                    .Select(t => t.Status.ToString())
                    .FirstOrDefault() ?? "None"
            }).ToList()
        };
    }
    
    private static string[] NameTokens(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Array.Empty<string>();
        var cleaned = new string(
            name.ToUpperInvariant()
                .Select(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) ? c : ' ')
                .ToArray());
        return cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool NamesMatch(string userName, string accountName)
    {
        var user = NameTokens(userName);
        if (user.Length == 0) return false;
        var account = NameTokens(accountName).ToHashSet(StringComparer.Ordinal);
        return user.All(account.Contains);
    }
}