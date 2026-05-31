using System;
using FluentResults;

namespace ECommerce.ProductService.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public ICollection<Product> Products { get; private set; } = new List<Product>();

	private Category() { } // EF Core constructor

    private Category(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Category> Create(string name, string description)
    {
        var result = new Result<Category>();

        if (string.IsNullOrWhiteSpace(name))
            result.WithError("Category name cannot be empty.");

        if (result.IsFailed)
            return result;

        var category = new Category(Guid.NewGuid(), name, description);
        return Result.Ok(category);
    }

    public Result Update(string name, string description)
    {
        var result = new Result();

        if (string.IsNullOrWhiteSpace(name))
            result.WithError("Category name cannot be empty.");

        if (result.IsFailed)
            return result;

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        return Result.Ok();
    }
}
