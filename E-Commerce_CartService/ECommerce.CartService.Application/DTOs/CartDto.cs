using System;
using System.Collections.Generic;
using System.Linq;

namespace ECommerce.CartService.Application.DTOs;

public record CartDto(
    Guid Id,
    string UserId,
    List<CartItemDto> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
}
