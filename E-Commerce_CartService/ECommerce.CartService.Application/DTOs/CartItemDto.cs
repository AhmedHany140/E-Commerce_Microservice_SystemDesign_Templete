using System;

namespace ECommerce.CartService.Application.DTOs;

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    DateTime AddedAt);
