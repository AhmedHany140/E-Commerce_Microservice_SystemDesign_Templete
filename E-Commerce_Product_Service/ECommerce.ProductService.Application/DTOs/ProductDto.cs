using System;

namespace ECommerce.ProductService.Application.DTOs;

public record ProductDto
{
	public Guid Id { get; init; }
	public string Name { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
	public decimal Price { get; init; }
	public int StockQuantity { get; init; }
	public Guid CategoryId { get; init; }
	public CategoryDto? Category { get; init; }
	public DateTime CreatedAt { get; init; }
	public DateTime? UpdatedAt { get; init; }
}
