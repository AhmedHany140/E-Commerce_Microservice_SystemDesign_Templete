using System;

namespace ECommerce.CartService.Application.Common.Models;

public record ProductDto(Guid Id, string Name,
	string Description,decimal Price,int StockQuantity);
