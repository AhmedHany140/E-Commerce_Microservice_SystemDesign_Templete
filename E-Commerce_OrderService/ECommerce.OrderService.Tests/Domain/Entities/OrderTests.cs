using System;
using System.Collections.Generic;
using Bogus;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ECommerce.OrderService.Tests.Domain.Entities;

public class OrderTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Create_WithValidParameters_ReturnsSuccessAndOrder()
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var shippingAddress = _faker.Address.FullAddress();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), _faker.Commerce.ProductName(), _faker.Random.Decimal(10, 100), _faker.Random.Int(1, 5))
        };

        // Act
        var result = Order.Create(userId, shippingAddress, items);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(userId);
        result.Value.ShippingAddress.Should().Be(shippingAddress);
        result.Value.Status.Should().Be(OrderStatus.Pending);
        result.Value.PaymentStatus.Should().Be(PaymentStatus.Pending);
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalPrice.Should().Be(items[0].UnitPrice * items[0].Quantity);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidUserId_ReturnsFailure(string? invalidUserId)
    {
        // Arrange
        var shippingAddress = _faker.Address.FullAddress();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), _faker.Commerce.ProductName(), 10.0m, 1)
        };

        // Act
        var result = Order.Create(invalidUserId!, shippingAddress, items);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("UserId is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidShippingAddress_ReturnsFailure(string? invalidShippingAddress)
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), _faker.Commerce.ProductName(), 10.0m, 1)
        };

        // Act
        var result = Order.Create(userId, invalidShippingAddress!, items);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Shipping address is required.");
    }

    [Fact]
    public void Create_WithNullItems_ReturnsFailure()
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var shippingAddress = _faker.Address.FullAddress();

        // Act
        var result = Order.Create(userId, shippingAddress, null!);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Order must have at least one item.");
    }

    [Fact]
    public void Create_WithEmptyItems_ReturnsFailure()
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var shippingAddress = _faker.Address.FullAddress();
        var items = new List<OrderItem>();

        // Act
        var result = Order.Create(userId, shippingAddress, items);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Order must have at least one item.");
    }

    [Fact]
    public void UpdateStatus_WithNewStatus_UpdatesStatusSuccessfully()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var result = order.UpdateStatus(OrderStatus.Confirmed);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Cancel_WhenStatusIsPending_CancelsOrderSuccessfully()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var result = order.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Cancel_WhenStatusIsNotPending_ReturnsFailure(OrderStatus status)
    {
        // Arrange
        var order = CreateValidOrder();
        order.UpdateStatus(status);

        // Act
        var result = order.Cancel();

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Only pending orders can be cancelled.");
        order.Status.Should().Be(status); // status remains unchanged
    }

    [Fact]
    public void MarkAsPaid_SetsPaymentStatusToPaid()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.MarkAsPaid();

        // Assert
        order.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public void MarkAsPaymentFailed_SetsPaymentStatusToFailed()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        order.MarkAsPaymentFailed();

        // Assert
        order.PaymentStatus.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void SetPaymobTransactionId_SetsTransactionIdCorrectly()
    {
        // Arrange
        var order = CreateValidOrder();
        var txId = _faker.Random.AlphaNumeric(10);

        // Act
        order.SetPaymobTransactionId(txId);

        // Assert
        order.PaymobTransactionId.Should().Be(txId);
    }

    [Fact]
    public void MarkAsRefunded_SetsPaymentStatusToRefundedAndSetsRefundTransactionId()
    {
        // Arrange
        var order = CreateValidOrder();
        var refundTxId = _faker.Random.AlphaNumeric(10);

        // Act
        order.MarkAsRefunded(refundTxId);

        // Assert
        order.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        order.RefundTransactionId.Should().Be(refundTxId);
    }

    private Order CreateValidOrder()
    {
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), _faker.Commerce.ProductName(), 25.50m, 2)
        };
        return Order.Create(_faker.Random.Guid().ToString(), _faker.Address.FullAddress(), items).Value;
    }
}
