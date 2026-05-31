using ECommerce.OrderService.Api.DTOs;
using ECommerce.OrderService.Infrastructure.Persistence;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce.OrderService.Api.Queries;

public class Query
{
	[Authorize(Roles = new[] { "Admin" })]
	[UsePaging(IncludeTotalCount = true)]
	[UseProjection]
	[UseFiltering]
	[UseSorting]
	public IQueryable<OrderDto> GetOrders(
		[Service] OrderDbContext dbContext)
	{
		return dbContext.Orders
			.Include(x => x.Items)
			.Select(order => order.ToDto());
	}

	[Authorize(Roles = new[] { "Customer" })]
	[UsePaging(IncludeTotalCount = true)]
	[UseProjection]
	[UseFiltering]
	[UseSorting]
	public IQueryable<OrderDto> GetMyOrders(
		[Service] IHttpContextAccessor httpContextAccessor,
		[Service] OrderDbContext dbContext)
	{
		var userId = httpContextAccessor.HttpContext?.User
			?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		return dbContext.Orders
			.Include(x => x.Items)
			.Where(x => x.UserId == userId)
			.Select(order => order.ToDto());
	}

	[Authorize(Roles = new[] { "Customer", "Admin" })]
	public async Task<OrderDto?> GetOrderById(
		Guid id,
		[Service] OrderDbContext dbContext)
	{
		var order = await dbContext.Orders
			.Include(x => x.Items)
			.FirstOrDefaultAsync(x => x.Id == id);

		if (order is null)
			return null;

		return order.ToDto();
	}
}