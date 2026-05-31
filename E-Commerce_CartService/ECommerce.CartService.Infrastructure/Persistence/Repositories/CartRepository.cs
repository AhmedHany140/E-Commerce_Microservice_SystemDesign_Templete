using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Application.Common.Interfaces;
using ECommerce.CartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.CartService.Infrastructure.Persistence.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext _context;

    public CartRepository(CartDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(Cart cart, CancellationToken ct = default)
    {
        await _context.Carts.AddAsync(cart, ct);
    }

    public void Update(Cart cart)
    {
        _context.Carts.Update(cart);
    }

    public void Delete(Cart cart)
    {
        _context.Carts.Remove(cart);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
