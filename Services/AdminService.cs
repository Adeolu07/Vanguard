using _Tripfinity.Interfaces;
using _Tripfinity.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace _Tripfinity.Services;

public class AdminService: IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<string?> GetAdminWalletIdAsync()
    {
        var admin = await _context.Users.FirstOrDefaultAsync(user => user.Role == "Admin");
        return admin?.UserWalletId;
    }
    
    
}