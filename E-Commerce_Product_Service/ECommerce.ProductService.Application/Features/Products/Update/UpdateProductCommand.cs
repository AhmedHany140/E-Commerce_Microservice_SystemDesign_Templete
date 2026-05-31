using System;

namespace ECommerce.ProductService.Application.Features.Products.Update;

public record UpdateProductCommand(Guid Id, string Name, string Description, decimal Price, int StockQuantity, Guid CategoryId);
