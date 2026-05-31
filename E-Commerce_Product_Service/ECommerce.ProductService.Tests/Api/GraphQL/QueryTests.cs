using System;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.ProductService.Api.GraphQL;
using ECommerce.ProductService.Infrastructure.Persistence;
using ECommerce.ProductService.Tests.Helpers;
using FluentAssertions;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.ProductService.Tests.Api.GraphQL;

public class QueryTests
{
    private readonly DbContextOptions<ProductDbContext> _options;

    public QueryTests()
    {
        _options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void GetProducts_ReturnsAllProductsMappedToDto()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var category = CategoryFaker.Generate();
        var product1 = ProductFaker.Generate(categoryId: category.Id);
        var product2 = ProductFaker.Generate(categoryId: category.Id);
        context.Categories.Add(category);
        context.Products.AddRange(product1, product2);
        context.SaveChanges();

        var query = new Query();

        // Act
        var result = query.GetProducts(context).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().Contain(new[] { product1.Id, product2.Id });
        var firstDto = result.First(x => x.Id == product1.Id);
        firstDto.Name.Should().Be(product1.Name);
        firstDto.Category.Should().NotBeNull();
        firstDto.Category!.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ReturnsProductDto()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var category = CategoryFaker.Generate();
        var product = ProductFaker.Generate(categoryId: category.Id);
        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var query = new Query();

        // Act
        var result = await query.GetProductById(context, product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.Category.Should().NotBeNull();
        result.Category!.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ThrowsGraphQLException()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var query = new Query();
        var nonexistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await query.GetProductById(context, nonexistentId);

        // Assert
        await act.Should().ThrowAsync<GraphQLException>()
            .WithMessage("Product With this Id Not Found");
    }

    [Fact]
    public void GetCategories_ReturnsAllCategoriesMappedToDto()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var category1 = CategoryFaker.Generate();
        var category2 = CategoryFaker.Generate();
        context.Categories.AddRange(category1, category2);
        context.SaveChanges();

        var query = new Query();

        // Act
        var result = query.GetCategories(context).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().Contain(new[] { category1.Id, category2.Id });
    }

    [Fact]
    public async Task GetCategoryById_WithValidId_ReturnsCategoryDto()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var category = CategoryFaker.Generate();
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var query = new Query();

        // Act
        var result = await query.GetCategoryById(context, category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be(category.Name);
    }

    [Fact]
    public async Task GetCategoryById_WithInvalidId_ThrowsGraphQLException()
    {
        // Arrange
        using var context = new ProductDbContext(_options);
        var query = new Query();
        var nonexistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await query.GetCategoryById(context, nonexistentId);

        // Assert
        await act.Should().ThrowAsync<GraphQLException>()
            .WithMessage("Category With this Id Not Found");
    }
}
