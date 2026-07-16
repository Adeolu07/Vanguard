using _Tripfinity.Models;
using _Tripfinity.Models.Data;
using _Tripfinity.Models.Tables;
using _Tripfinity.Views;
using _Tripfinity.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[ApiController]
[Route("api/railwaytrips")]
public class RailwayTripsApi : ControllerBase
{
    private readonly AppDbContext _context;

    public RailwayTripsApi(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveTrainTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 9)
    {
        var query = _context.RailwayTrips
            .Where(trip => trip.IsActive && trip.DepartureTime > DateTime.Now )
            .OrderBy(t => t.DepartureTime);

        var paginatedList = await PaginatedList<RailwayTrip>.CreateAsync(query, page, pageSize);

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
    public async Task<IActionResult> GetRailwayTrip(int id)
    {
        var trip = await _context.RailwayTrips.FindAsync(id);

        if (trip == null)
            return NotFound(new ErrorResponse
            {
                Success = false,
                Message = $"Train Trip with id {id} not found.",
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