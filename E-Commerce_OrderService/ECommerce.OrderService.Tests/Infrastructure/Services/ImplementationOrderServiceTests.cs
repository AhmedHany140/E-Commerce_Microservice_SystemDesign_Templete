using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Domain.Enums;
using ECommerce.OrderService.Infrastructure.Grpc;
using ECommerce.OrderService.Infrastructure.Services;
using ECommerce.OrderService.Tests.Helpers;
using FluentAssertions;
using FluentResults;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace ECommerce.OrderService.Tests.Infrastructure.Services;

public class ImplementationOrderServiceTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartServiceClient _cartServiceClient;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ImplementationOrderService _service;
    private readonly Faker _faker = new();
    private readonly ServerCallContext _context;

    public ImplementationOrderServiceTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _cartServiceClient = Substitute.For<ICartServiceClient>();
        _productServiceClient = Substitute.For<IProductServiceClient>();
        _context = Substitute.For<ServerCallContext>();

        _service = new ImplementationOrderService(
            _orderRepository,
            _cartServiceClient,
            _productServiceClient);
    }

    #region PaidOrder Tests

    [Fact]
    public async Task PaidOrder_WithValidUnpaidOrder_MarksAsPaidAndSaves()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new PayOrderRequest { OrderId = orderId.ToString() };
        var order = TestEntityFactory.CreateOrderWithPrivateFields(
            orderId, "user-1", 100.0m, OrderStatus.Pending, "Address", PaymentStatus.Pending, DateTime.UtcNow, new List<OrderItem>());

        _orderRepository.GetByIdAsync(orderId).Returns(order);

        // Act
        var response = await _service.PaidOrder(request, _context);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        order.PaymentStatus.Should().Be(PaymentStatus.Paid);

        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task PaidOrder_WhenAlreadyPaid_ReturnsSuccessWithoutSaving()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new PayOrderRequest { OrderId = orderId.ToString() };
        var order = TestEntityFactory.CreateOrderWithPrivateFields(
            orderId, "user-1", 100.0m, OrderStatus.Pending, "Address", PaymentStatus.Paid, DateTime.UtcNow, new List<OrderItem>());

        _orderRepository.GetByIdAsync(orderId).Returns(order);

        // Act
        var response = await _service.PaidOrder(request, _context);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();

        await _orderRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task PaidOrder_WhenOrderNotFound_ThrowsRpcExceptionWithNotFoundStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new PayOrderRequest { OrderId = orderId.ToString() };
        _orderRepository.GetByIdAsync(orderId).Returns((Order?)null);

        // Act
        Func<Task> act = async () => await _service.PaidOrder(request, _context);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.And.StatusCode.Should().Be(StatusCode.NotFound);
        exception.And.Status.Detail.Should().Be("Order not found");
    }

    [Fact]
    public async Task PaidOrder_WhenDbExceptionOccurs_ThrowsRpcExceptionWithInternalStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new PayOrderRequest { OrderId = orderId.ToString() };
        _orderRepository.GetByIdAsync(orderId).Throws(new Exception("Database connection failed"));

        // Act
        Func<Task> act = async () => await _service.PaidOrder(request, _context);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.And.StatusCode.Should().Be(StatusCode.Internal);
        exception.And.Status.Detail.Should().Be("Error while processing payment");
    }

    #endregion

    #region CreateOrder Tests

    [Fact]
    public async Task CreateOrder_WithValidRequestAndCart_CreatesAndSavesOrder()
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var shippingAddress = _faker.Address.FullAddress();
        var request = new CreateOrderRequest { UserId = userId, ShippingAddress = shippingAddress };

        var productId1 = Guid.NewGuid();
        var cartItems = new List<CartItemDto>
        {
            new("item-1", "cart-id", productId1.ToString(), 2, DateTime.UtcNow)
        };
        var cart = new CartDto("cart-id", userId, DateTime.UtcNow, cartItems);

        _cartServiceClient.GetCartAsync(userId).Returns(Result.Ok(cart));

        var productDto = new ProductDto(productId1, "Product 1", "Desc 1", 20.0m, 100);
        _productServiceClient.GetProductAsync(productId1).Returns(Result.Ok(productDto));

        // Act
        var response = await _service.CreateOrder(request, _context);

        // Assert
        response.Should().NotBeNull();
        response.OrderId.Should().NotBeEmpty();
        response.TotalPrice.Should().Be(40.0); // 2 * 20.0

        await _orderRepository.Received(1).AddAsync(Arg.Is<Order>(o =>
            o.UserId == userId &&
            o.ShippingAddress == shippingAddress &&
            o.TotalPrice == 40.0m
        ));
        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateOrder_WhenCartNotFound_ThrowsRpcExceptionWithNotFoundStatus()
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var request = new CreateOrderRequest { UserId = userId, ShippingAddress = "Address" };

        _cartServiceClient.GetCartAsync(userId).Returns(Result.Fail("Cart not found"));

        // Act
        Func<Task> act = async () => await _service.CreateOrder(request, _context);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.And.StatusCode.Should().Be(StatusCode.NotFound);
        exception.And.Status.Detail.Should().Be("Cart not found");
    }

    [Fact]
    public async Task CreateOrder_WhenCartIsEmpty_ThrowsRpcExceptionWithNotFoundStatus()
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var request = new CreateOrderRequest { UserId = userId, ShippingAddress = "Address" };
        var cart = new CartDto("cart-id", userId, DateTime.UtcNow, new List<CartItemDto>());

        _cartServiceClient.GetCartAsync(userId).Returns(Result.Ok(cart));

        // Act
        Func<Task> act = async () => await _service.CreateOrder(request, _context);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.And.StatusCode.Should().Be(StatusCode.NotFound);
        exception.And.Status.Detail.Should().Be("Cart is Empty");
    }

    [Fact]
    public async Task CreateOrder_WhenProductNotFound_ThrowsRpcExceptionWithNotFoundStatus()
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var request = new CreateOrderRequest { UserId = userId, ShippingAddress = "Address" };

        var productId = Guid.NewGuid();
        var cartItems = new List<CartItemDto>
        {
            new("item-1", "cart-id", productId.ToString(), 2, DateTime.UtcNow)
        };
        var cart = new CartDto("cart-id", userId, DateTime.UtcNow, cartItems);

        _cartServiceClient.GetCartAsync(userId).Returns(Result.Ok(cart));
        _productServiceClient.GetProductAsync(productId).Returns(Result.Fail("Product not found"));

        // Act
        Func<Task> act = async () => await _service.CreateOrder(request, _context);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.And.StatusCode.Should().Be(StatusCode.NotFound);
        exception.And.Status.Detail.Should().Contain("not found or unavailable");
    }

    [Fact]
    public async Task CreateOrder_WhenOrderCreationFails_ThrowsRpcExceptionWithNotFoundStatus()
    {
        // Arrange
        var userId = ""; // triggers validation failure in Order.Create
        var request = new CreateOrderRequest { UserId = userId, ShippingAddress = "Address" };

        var productId = Guid.NewGuid();
        var cartItems = new List<CartItemDto>
        {
            new("item-1", "cart-id", productId.ToString(), 2, DateTime.UtcNow)
        };
        var cart = new CartDto("cart-id", userId, DateTime.UtcNow, cartItems);

        _cartServiceClient.GetCartAsync(userId).Returns(Result.Ok(cart));

        var productDto = new ProductDto(productId, "Product 1", "Desc 1", 20.0m, 100);
        _productServiceClient.GetProductAsync(productId).Returns(Result.Ok(productDto));

        // Act
        Func<Task> act = async () => await _service.CreateOrder(request, _context);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.And.StatusCode.Should().Be(StatusCode.NotFound);
        exception.And.Status.Detail.Should().Be("Create Order Faild");
    }

    #endregion
}
