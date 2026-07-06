using _Tripfinity.Models.Data;
using _Tripfinity.Models.Tables;
using _Tripfinity.Utilities;
using _Tripfinity.Views;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[Route("api/taxi")]
[ApiController]
public class TaxiTripsApi : ControllerBase
{
    private readonly AppDbContext _context;

    public TaxiTripsApi(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTaxiTrips(int page = 1, int pageSize = 10, string sort = "time")
    {
        var query = _context.TaxiTrips
            .Where(t => t.IsActive && t.PickupTime > DateTime.Now)
            .OrderBy(t => t.PickupTime);

        // query = sort switch
        // {
        //     "price" => query.OrderBy(t => t.Price),
        //     _ => query.OrderBy(t => t.PickupTime)
        // };

        var paginatedList = await PaginatedList<TaxiTrip>.CreateAsync(query, page, pageSize);
        return Ok(new
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
    public async Task<IActionResult> GetTaxiTrip(int id)
    {
        var trip = await _context.TaxiTrips.FindAsync(id);

        if (trip == null)
            return NotFound(new ErrorResponse
            {
                Success = false,
                Message = $"Taxi Trip with id {id} not found.",
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