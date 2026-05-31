using Bogus;
using ECommerce.ProductService.Domain.Entities;
using System;

namespace ECommerce.ProductService.Tests.Helpers;

public static class ProductFaker
{
    private static readonly Faker Faker = new();

    public static Product Generate(Guid? id = null, Guid? categoryId = null)
    {
        return TestEntityFactory.CreateProduct(
            id ?? Guid.NewGuid(),
            Faker.Commerce.ProductName(),
            Faker.Lorem.Sentence(),
            Faker.Finance.Amount(5, 1000),
            Faker.Random.Number(0, 500),
            categoryId ?? Guid.NewGuid()
        );
    }
}
