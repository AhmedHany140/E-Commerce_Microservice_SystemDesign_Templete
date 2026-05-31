using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Domain.Entities;

namespace ECommerce.CartService.Application.Common.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Cart cart, CancellationToken ct = default);
    void Update(Cart cart);
    void Delete(Cart cart);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
