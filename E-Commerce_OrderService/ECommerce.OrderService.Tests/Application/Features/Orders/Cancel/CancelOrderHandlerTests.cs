using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.OrderService.Application.Features.Orders.Cancel;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Domain.Enums;
using ECommerce.OrderService.Tests.Helpers;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ECommerce.OrderService.Tests.Application.Features.Orders.Cancel;

public class CancelOrderHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly ILogger _logger;
    private readonly Faker _faker = new();

    public CancelOrderHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _paymentServiceClient = Substitute.For<IPaymentServiceClient>();
        _logger = Substitute.For<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidPendingUnpaidOrder_CancelsOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = _faker.Random.Guid().ToString();
        var command = new CancelOrderCommand(orderId, userId);
        var ct = CancellationToken.None;

        var order = TestEntityFactory.CreateOrderWithPrivateFields(
            orderId, userId, 100.0m, OrderStatus.Pending, "Address", PaymentStatus.Pending, DateTime.UtcNow, new List<OrderItem>());

        _orderRepository.GetByIdAsync(orderId, ct).Returns(order);

        // Act
        var result = await CancelOrderHandler.Handle(command, _orderRepository, _paymentServiceClient, _logger, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);

        _orderRepository.Received(1).Update(order);
        await _paymentServiceClient.DidNotReceiveWithAnyArgs().RefundAsync(null!, ct);
    }

    [Fact]
    public async Task Handle_WithValidPendingPaidOrderAndTransactionId_RefundsAndCancelsSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = _faker.Random.Guid().ToString();
        var txId = "tx-12345";
        var refundTxId = "refund-98765";
        var command = new CancelOrderCommand(orderId, userId);
        var ct = CancellationToken.None;

        var order = TestEntityFactory.CreateOrderWithPrivateFields(
            orderId, userId, 150.0m, OrderStatus.Pending, "Address", PaymentStatus.Paid, DateTime.UtcNow, new List<OrderItem>(), txId);

        _orderRepository.GetByIdAsync(orderId, ct).Returns(order);

        var refundDto = new RefundDto(true, "Refund succeeded", refundTxId);
        _paymentServiceClient.RefundAsync(Arg.Is<RefundPaymentRequest>(r => r.PaymobTransactionId == txId && r.Amount == 150.0m), ct)
            .Returns(Result.Ok(refundDto));

        // Act
        var result = await CancelOrderHandler.Handle(command, _orderRepository, _paymentServiceClient, _logger, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        order.RefundTransactionId.Should().Be(refundTxId);

        _orderRepository.Received(1).Update(order);

        // Verify Logging
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Initiating refund")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Refund successful")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenPaidOrderHasNoTransactionId_SkipsRefundAndCancelsSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = _faker.Random.Guid().ToString();
        var command = new CancelOrderCommand(orderId, userId);
        var ct = CancellationToken.None;

        var order = TestEntityFactory.CreateOrderWithPrivateFields(
            orderId, userId, 150.0m, OrderStatus.Pending, "Address", PaymentStatus.Paid, DateTime.UtcNow, new List<OrderItem>(), null);

        _orderRepository.GetByIdAsync(orderId, ct).Returns(order);

        // Act
        var result = await CancelOrderHandler.Handle(command, _orderRepository, _paymentServiceClient, _logger, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.PaymentStatus.Should().Be(PaymentStatus.Paid); // Remains Paid since no refund could be initiated

        _orderRepository.Received(1).Update(order);
        await _paymentServiceClient.DidNotReceiveWithAnyArgs().RefundAsync(null!, ct);

        // Verify Warning Log
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("skipping refund")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenRefundFails_ReturnsFailureAndSkipsCancellationSave()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = _faker.Random.Guid().ToString();
        var txId = "tx-12345";
        var command = new CancelOrderCommand(orderId, userId);
        var ct = CancellationToken.None;

        var order = TestEntityFactory.CreateOrderWithPrivateFields(
            orderId, userId, 150.0m, OrderStatus.Pending, "Address", PaymentStatus.Paid, DateTime.UtcNow, new List<OrderItem>(), txId);

        _orderRepository.GetByIdAsync(orderId, ct).Returns(order);

        _paymentServiceClient.RefundAsync(Arg.Any<RefundPaymentRequest>(), ct)
            .Returns(Result.Fail("Payment gateway timeout."));

        // Act
        var result = await CancelOrderHandler.Handle(command, _orderRepository, _paymentServiceClient, _logger, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Refund failed: Payment gateway timeout.");

        order.Status.Should().Be(OrderStatus.Cancelled); // Mutated in-memory but not saved to DB
        _orderRepository.DidNotReceive().Update(order);

        // Verify Error Log
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Refund failed")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CancelOrderCommand(orderId, "userId");
        var ct = CancellationToken.None;

        _orderRepository.GetByIdAsync(orderId, ct).Returns((Order?)null);

        // Act
        var result = await CancelOrderHandler.Handle(command, _orderRepository, _paymentServiceClient, _logger, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Order not found.");
    }

    [Fact]
    public async Task Handle_WhenUserIsUnauthorized_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CancelOrderCommand(orderId, "unauthorized-user");
        var ct = CancellationToken.None;

        var order = TestEntityFactory.CreateOrderWithPrivateFields(
            orderId, "authorized-user", 100.0m, OrderStatus.Pending, "Address", PaymentStatus.Pending, DateTime.UtcNow, new List<OrderItem>());

        _orderRepository.GetByIdAsync(orderId, ct).Returns(order);

        // Act
        var result = await CancelOrderHandler.Handle(command, _orderRepository, _paymentServiceClient, _logger, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Unauthorized to cancel this order.");
    }

    [Fact]
    public async Task Handle_WhenOrderIsNotPending_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = "user-123";
        var command = new CancelOrderCommand(orderId, userId);
        var ct = CancellationToken.None;

        var order = TestEntityFactory.CreateOrderWithPrivateFields(
            orderId, userId, 100.0m, OrderStatus.Shipped, "Address", PaymentStatus.Pending, DateTime.UtcNow, new List<OrderItem>());

        _orderRepository.GetByIdAsync(orderId, ct).Returns(order);

        // Act
        var result = await CancelOrderHandler.Handle(command, _orderRepository, _paymentServiceClient, _logger, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Only pending orders can be cancelled.");
    }
}
