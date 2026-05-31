using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Bogus;
using ECommerce.OrderService.Api.DTOs;
using ECommerce.OrderService.Api.Queries;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Infrastructure.Persistence;
using ECommerce.OrderService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace ECommerce.OrderService.Tests.Api.Queries;

public class QueryTests
{
    private readonly DbContextOptions<OrderDbContext> _options;
    private readonly Faker _faker = new();

    public QueryTests()
    {
        _options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void GetOrders_ReturnsAllOrdersMappedToDto()
    {
        // Arrange
        using var context = new OrderDbContext(_options);
        var order1 = CreateTestOrder("user-1");
        var order2 = CreateTestOrder("user-2");
        context.Orders.AddRange(order1, order2);
        context.SaveChanges();

        var query = new Query();

        // Act
        var result = query.GetOrders(context).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Any(o => o.Id == order1.Id).Should().BeTrue();
        result.Any(o => o.Id == order2.Id).Should().BeTrue();
        result[0].Items.Should().NotBeEmpty();
    }

    [Fact]
    public void GetMyOrders_ReturnsOnlyCurrentUserOrdersMappedToDto()
    {
        // Arrange
        var targetUserId = "user-target";
        var otherUserId = "user-other";

        using var context = new OrderDbContext(_options);
        var order1 = CreateTestOrder(targetUserId);
        var order2 = CreateTestOrder(otherUserId);
        context.Orders.AddRange(order1, order2);
        context.SaveChanges();

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, targetUserId) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        httpContextAccessor.HttpContext.Returns(httpContext);

        var query = new Query();

        // Act
        var result = query.GetMyOrders(httpContextAccessor, context).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(order1.Id);
        result[0].UserId.Should().Be(targetUserId);
    }

    [Fact]
    public async Task GetOrderById_WhenOrderExists_ReturnsOrderDto()
    {
        // Arrange
        using var context = new OrderDbContext(_options);
        var order = CreateTestOrder("user-1");
        context.Orders.Add(order);
        context.SaveChanges();

        var query = new Query();

        // Act
        var result = await query.GetOrderById(order.Id, context);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task GetOrderById_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = new OrderDbContext(_options);
        var query = new Query();

        // Act
        var result = await query.GetOrderById(Guid.NewGuid(), context);

        // Assert
        result.Should().BeNull();
    }

    private Order CreateTestOrder(string userId)
    {
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), _faker.Commerce.ProductName(), 10.0m, 1)
        };
        return Order.Create(userId, _faker.Address.FullAddress(), items).Value;
    }
}
