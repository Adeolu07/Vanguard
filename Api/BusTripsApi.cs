using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Utilities;
using _Tripfinity.Views;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[Route("api/bustrips")]
[ApiController]
public class BusTripsApi : ControllerBase
{
    private readonly AppDbContext _context;

    public BusTripsApi(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveBusTrips(int page = 1, int pageSize = 10)
    {
        var query = _context.BusTrips
            .Where(trip => trip.IsActive && trip.DepartureTime > DateTime.Now)
            .OrderBy(trip => trip.Id);

        var paginatedList = await PaginatedList<BusTrip>.CreateAsync(query, page, pageSize);

        return Ok(
            new
            {
                success = true,
                data = paginatedList,
                pagination = new
                {
                    pageIndex = paginatedList.PageIndex,
                    totalPages = paginatedList.TotalPages,
                    totalCount = paginatedList.TotalCount,
                    hasPreviousPage = paginatedList.HasPreviousPage,
                    hasNextPage = paginatedList.HasNextPage
                }
            });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBusTrip(int id)
    {
        var trip = await _context.BusTrips.FindAsync(id);

        if (trip == null)
            return NotFound(new ErrorResponse
                {
                    Success =  false,
                    Message = $"Bus Trip with id {id} not found.",
                    ErrorCode = "404"
                });

        return Ok(
            new
            {
                success = true,
                data = trip
            });
    }
}