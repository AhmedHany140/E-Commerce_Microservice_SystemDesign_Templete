using System;
using Bogus;
using ECommerce.OrderService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ECommerce.OrderService.Tests.Domain.Entities;

public class OrderItemTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Create_WithValidParameters_ReturnsOrderItemWithCorrectValues()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = _faker.Commerce.ProductName();
        var unitPrice = _faker.Random.Decimal(5, 500);
        var quantity = _faker.Random.Int(1, 10);

        // Act
        var orderItem = OrderItem.Create(productId, productName, unitPrice, quantity);

        // Assert
        orderItem.Should().NotBeNull();
        orderItem.Id.Should().NotBeEmpty();
        orderItem.ProductId.Should().Be(productId);
        orderItem.ProductName.Should().Be(productName);
        orderItem.UnitPrice.Should().Be(unitPrice);
        orderItem.Quantity.Should().Be(quantity);
    }
}
