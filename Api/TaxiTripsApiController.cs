using _Tripfinity.Models.Data;
using _Tripfinity.Models.Tables;
using _Tripfinity.Views;
using Microsoft.AspNetCore.Mvc;

namespace _Tripfinity.Api;

[Route("api/taxitrips")]
[ApiController]
public class TaxiTripsApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public TaxiTripsApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTaxiTrips(int page = 1, int pageSize = 10, string sort = "time")
    {
        var query = _context.TaxiTrips
            .Where(t => t.IsActive && t.PickupTime > DateTime.Now)
            .AsQueryable();

        query = sort switch
        {
            "price" => query.OrderBy(t => t.Price),
            _ => query.OrderBy(t => t.PickupTime)
        };

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
            return NotFound(new { success = false, message = "Taxi trip not found" });

        return Ok(new { success = true, data = trip });
    }
}