using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Domain.Enums;
using ECommerce.OrderService.Infrastructure.Persistence;
using ECommerce.OrderService.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.OrderService.Tests.Infrastructure.Persistence;

public class OrderRepositoryTests
{
    private readonly DbContextOptions<OrderDbContext> _options;
    private readonly Faker _faker = new();

    public OrderRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task AddAsync_WithValidOrder_SavesOrderToDatabase()
    {
        // Arrange
        using var context = new OrderDbContext(_options);
        var repository = new OrderRepository(context);

        var order = CreateTestOrder("user-1");

        // Act
        await repository.AddAsync(order);
        await repository.SaveChangesAsync();

        // Assert
        using var assertContext = new OrderDbContext(_options);
        var dbOrder = await assertContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == order.Id);

        dbOrder.Should().NotBeNull();
        dbOrder!.UserId.Should().Be("user-1");
        dbOrder.Items.Should().HaveCount(1);
        dbOrder.Items[0].ProductName.Should().Be(order.Items[0].ProductName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrderWithItems()
    {
        // Arrange
        using var context = new OrderDbContext(_options);
        var order = CreateTestOrder("user-2");
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = new OrderDbContext(_options);
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ReturnsUsersOrdersOrderedByDateDescending()
    {
        // Arrange
        using var context = new OrderDbContext(_options);
        var userId = "user-3";

        var orderOld = CreateTestOrder(userId);
        var orderNew = CreateTestOrder(userId);

        // Set different creation dates to verify ordering
        // (Note: In Memory DB does not block setting private fields if we use reflection,
        //  but actually we can just add them sequentially, or use our reflection helper)
        // Let's use our reflection helper in TestEntityFactory
        var orderOldWithPrivate = Helpers.TestEntityFactory.CreateOrderWithPrivateFields(
            Guid.NewGuid(), userId, 100m, OrderStatus.Pending, "Addr", PaymentStatus.Pending, DateTime.UtcNow.AddDays(-1), new List<OrderItem>());
        var orderNewWithPrivate = Helpers.TestEntityFactory.CreateOrderWithPrivateFields(
            Guid.NewGuid(), userId, 100m, OrderStatus.Pending, "Addr", PaymentStatus.Pending, DateTime.UtcNow, new List<OrderItem>());

        await context.Orders.AddRangeAsync(orderOldWithPrivate, orderNewWithPrivate);
        await context.SaveChangesAsync();

        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(orderNewWithPrivate.Id); // Newest first
        result[1].Id.Should().Be(orderOldWithPrivate.Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrdersOrderedByDateDescending()
    {
        // Arrange
        using var context = new OrderDbContext(_options);
        var order1 = Helpers.TestEntityFactory.CreateOrderWithPrivateFields(
            Guid.NewGuid(), "u1", 100m, OrderStatus.Pending, "Addr", PaymentStatus.Pending, DateTime.UtcNow.AddDays(-2), new List<OrderItem>());
        var order2 = Helpers.TestEntityFactory.CreateOrderWithPrivateFields(
            Guid.NewGuid(), "u2", 100m, OrderStatus.Pending, "Addr", PaymentStatus.Pending, DateTime.UtcNow, new List<OrderItem>());

        await context.Orders.AddRangeAsync(order1, order2);
        await context.SaveChangesAsync();

        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(order2.Id); // Newest first
        result[1].Id.Should().Be(order1.Id);
    }

    [Fact]
    public void Update_ModifiesOrderStateInContext()
    {
        // Arrange
        using var context = new OrderDbContext(_options);
        var order = CreateTestOrder("user-4");
        context.Orders.Add(order);
        context.SaveChanges();

        var repository = new OrderRepository(context);

        // Act
        order.UpdateStatus(OrderStatus.Shipped);
        repository.Update(order);
        context.SaveChanges();

        // Assert
        using var assertContext = new OrderDbContext(_options);
        var dbOrder = assertContext.Orders.Find(order.Id);
        dbOrder.Should().NotBeNull();
        dbOrder!.Status.Should().Be(OrderStatus.Shipped);
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
