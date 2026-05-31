using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.OrderService.Api.Endpoints;
using ECommerce.OrderService.Application.Features.Orders.Cancel;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Wolverine;
using Xunit;

namespace ECommerce.OrderService.Tests.Api.Endpoints;

public class OrderEndpointsTests
{
    private readonly IMessageBus _messageBus;
    private readonly CancellationToken _ct = CancellationToken.None;

    public OrderEndpointsTests()
    {
        _messageBus = Substitute.For<IMessageBus>();
    }

    [Fact]
    public async Task CancelOrder_WithValidUserAndSuccessfulCommand_ReturnsNoContent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = "user-123";
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        _messageBus.InvokeAsync<Result>(Arg.Is<CancelOrderCommand>(c => c.OrderId == orderId && c.UserId == userId), _ct)
            .Returns(Result.Ok());

        // Act
        var result = await OrderEndpoints.CancelOrder(orderId, httpContext, _messageBus, _ct);

        // Assert
        result.Should().BeOfType<NoContent>();
        var statusCodeResult = result as IStatusCodeHttpResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task CancelOrder_WhenUserClaimMissing_ReturnsUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext(); // No user claims set

        // Act
        var result = await OrderEndpoints.CancelOrder(orderId, httpContext, _messageBus, _ct);

        // Assert
        result.Should().BeOfType<UnauthorizedHttpResult>();
        var statusCodeResult = result as IStatusCodeHttpResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        await _messageBus.DidNotReceiveWithAnyArgs().InvokeAsync<Result>(null!, _ct);
    }

    [Fact]
    public async Task CancelOrder_WhenCommandFails_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = "user-123";
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var failureResult = Result.Fail("Only pending orders can be cancelled.");
        _messageBus.InvokeAsync<Result>(Arg.Any<CancelOrderCommand>(), _ct)
            .Returns(failureResult);

        // Act
        var result = await OrderEndpoints.CancelOrder(orderId, httpContext, _messageBus, _ct);

        // Assert
        var statusCodeResult = result as IStatusCodeHttpResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var value = result.GetType().GetProperty("Value")?.GetValue(result) as IEnumerable<IError>;
        value.Should().NotBeNull();
        var errorList = value!.ToList();
        errorList.Should().HaveCount(1);
        errorList[0].Message.Should().Be("Only pending orders can be cancelled.");
    }
}
