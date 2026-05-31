using Bogus;
using ECommerce.ProductService.Domain.Entities;
using System;

namespace ECommerce.ProductService.Tests.Helpers;

public static class CategoryFaker
{
    private static readonly Faker Faker = new();

    public static Category Generate(Guid? id = null)
    {
        return TestEntityFactory.CreateCategory(
            id ?? Guid.NewGuid(),
            Faker.Commerce.Categories(1)[0],
            Faker.Lorem.Sentence()
        );
    }
}
