using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Domain.Entities;
using ECommerce.ProductService.Infrastructure.Persistence;
using ECommerce.ProductService.Infrastructure.Persistence.Repositories;
using ECommerce.ProductService.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.ProductService.Tests.Infrastructure.Persistence.Repositories;

public class ProductRepositoryTests
{
    private readonly DbContextOptions<ProductDbContext> _options;

    public ProductRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsProduct()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var product = ProductFaker.Generate();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetByIdAsync(product.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var product1 = ProductFaker.Generate();
        var product2 = ProductFaker.Generate();
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().Contain(new[] { product1.Id, product2.Id });
    }

    [Fact]
    public async Task AddAsync_AddsProductToDatabase()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var product = ProductFaker.Generate();
        var repository = new ProductRepository(context);

        // Act
        await repository.AddAsync(product, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var savedProduct = await context.Products.FindAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task Update_ModifiesProductInDatabase()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var product = ProductFaker.Generate();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        // Act
        product.Update("Updated Phone", "Updated Desc", 1200m, 100, product.CategoryId);
        repository.Update(product);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        using var contextAssert = new ProductDbContext(_options);
        var savedProduct = await contextAssert.Products.FindAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("Updated Phone");
        savedProduct.Price.Should().Be(1200m);
    }

    [Fact]
    public async Task Delete_RemovesProductFromDatabase()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var product = ProductFaker.Generate();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        // Act
        repository.Delete(product);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        using var contextAssert = new ProductDbContext(_options);
        var savedProduct = await contextAssert.Products.FindAsync(product.Id);
        savedProduct.Should().BeNull();
    }
}
