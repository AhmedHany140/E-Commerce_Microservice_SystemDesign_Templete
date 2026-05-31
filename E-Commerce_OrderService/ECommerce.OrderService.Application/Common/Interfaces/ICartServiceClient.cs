using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;

namespace ECommerce.OrderService.Application.Common.Interfaces;

public interface ICartServiceClient
{
    Task<Result<CartDto>> GetCartAsync(string userId, CancellationToken ct = default);
}

public record CartDto(string Id, string UserId,
    DateTime CreatedAt, List<CartItemDto> Items);
public record CartItemDto(string Id,string CartId
    ,string ProductId, int Quantity,DateTime AddedAt);
