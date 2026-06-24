using _Tripfinity.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 9)
    {
        var query = _context.RailwayTrips
            .Where(t => t.IsActive)
            .OrderBy(t => t.DepartureTime);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var trips = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = trips,
            pagination = new
            {
                pageIndex = page,
                totalPages,
                totalCount,
                hasPreviousPage = page > 1,
                hasNextPage = page < totalPages
            }
        });
    }
}