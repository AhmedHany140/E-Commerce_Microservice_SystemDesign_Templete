using System;
using FluentResults;

namespace ECommerce.ProductService.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; }
	public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Product() { } // EF Core constructor

    private Product(Guid id, string name, string description, decimal price, int stockQuantity, Guid categoryId)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Product> Create(string name, string description, decimal price, int stockQuantity, Guid categoryId)
    {
        var result = new Result<Product>();

        if (string.IsNullOrWhiteSpace(name))
            result.WithError("Product name cannot be empty.");
        if (price < 0)
            result.WithError("Product price cannot be negative.");
        if (stockQuantity < 0)
            result.WithError("Stock quantity cannot be negative.");

        if (result.IsFailed)
            return result;

        var product = new Product(Guid.NewGuid(), name, description, price, stockQuantity, categoryId);
        return Result.Ok(product);
    }

    public Result Update(string name, string description, decimal price, int stockQuantity, Guid categoryId)
    {
        var result = new Result();

        if (string.IsNullOrWhiteSpace(name))
            result.WithError("Product name cannot be empty.");
        if (price < 0)
            result.WithError("Product price cannot be negative.");
        if (stockQuantity < 0)
            result.WithError("Stock quantity cannot be negative.");

        if (result.IsFailed)
            return result;

        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Ok();
    }
}
