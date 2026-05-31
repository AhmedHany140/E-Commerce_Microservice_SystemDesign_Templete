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

public class CategoryRepositoryTests
{
    private readonly DbContextOptions<ProductDbContext> _options;

    public CategoryRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsCategory()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var category = CategoryFaker.Generate();
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var repository = new CategoryRepository(context);

        // Act
        var result = await repository.GetByIdAsync(category.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be(category.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var repository = new CategoryRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var category1 = CategoryFaker.Generate();
        var category2 = CategoryFaker.Generate();
        context.Categories.AddRange(category1, category2);
        await context.SaveChangesAsync();

        var repository = new CategoryRepository(context);

        // Act
        var result = await repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().Contain(new[] { category1.Id, category2.Id });
    }

    [Fact]
    public async Task AddAsync_AddsCategoryToDatabase()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var category = CategoryFaker.Generate();
        var repository = new CategoryRepository(context);

        // Act
        await repository.AddAsync(category, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var savedCategory = await context.Categories.FindAsync(category.Id);
        savedCategory.Should().NotBeNull();
        savedCategory!.Name.Should().Be(category.Name);
    }

    [Fact]
    public async Task Update_ModifiesCategoryInDatabase()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var category = CategoryFaker.Generate();
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var repository = new CategoryRepository(context);

        // Act
        category.Update("Updated Name", "Updated Desc");
        repository.Update(category);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        using var contextAssert = new ProductDbContext(_options);
        var savedCategory = await contextAssert.Categories.FindAsync(category.Id);
        savedCategory.Should().NotBeNull();
        savedCategory!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Delete_RemovesCategoryFromDatabase()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var category = CategoryFaker.Generate();
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var repository = new CategoryRepository(context);

        // Act
        repository.Delete(category);
        await repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        using var contextAssert = new ProductDbContext(_options);
        var savedCategory = await contextAssert.Categories.FindAsync(category.Id);
        savedCategory.Should().BeNull();
    }
}
