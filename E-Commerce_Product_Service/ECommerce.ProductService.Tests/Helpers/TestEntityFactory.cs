using System;
using System.Reflection;
using ECommerce.ProductService.Domain.Entities;

namespace ECommerce.ProductService.Tests.Helpers;

public static class TestEntityFactory
{
    public static Category CreateCategory(Guid id, string name, string description)
    {
        var constructor = typeof(Category).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(Guid), typeof(string), typeof(string) },
            null);

        if (constructor == null)
            throw new InvalidOperationException("Private constructor for Category not found.");

        return (Category)constructor.Invoke(new object[] { id, name, description });
    }

    public static Product CreateProduct(Guid id, string name, string description, decimal price, int stockQuantity, Guid categoryId)
    {
        var constructor = typeof(Product).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(Guid), typeof(string), typeof(string), typeof(decimal), typeof(int), typeof(Guid) },
            null);

        if (constructor == null)
            throw new InvalidOperationException("Private constructor for Product not found.");

        return (Product)constructor.Invoke(new object[] { id, name, description, price, stockQuantity, categoryId });
    }
}
