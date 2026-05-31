using System;

namespace ECommerce.ProductService.Application.Features.Products.Create;

public record CreateProductCommand(string Name, string Description, decimal Price, int StockQuantity, Guid CategoryId);
