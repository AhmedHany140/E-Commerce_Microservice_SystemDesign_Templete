using System;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.OrderService.Infrastructure.Clients;
using ECommerce.PaymentService.Api.Grpc;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace ECommerce.OrderService.Tests.Infrastructure.Clients;

public class PaymentServiceClientTests
{
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _grpcClient;
    private readonly ILogger<PaymentServiceClient> _logger;
    private readonly PaymentServiceClient _client;
    private readonly Faker _faker = new();

    public PaymentServiceClientTests()
    {
        _grpcClient = Substitute.For<PaymentGrpcService.PaymentGrpcServiceClient>();
        _logger = Substitute.For<ILogger<PaymentServiceClient>>();
        _client = new PaymentServiceClient(_grpcClient, _logger);
    }

    [Fact]
    public async Task RefundAsync_WithValidRequest_ReturnsSuccessAndRefundDto()
    {
        // Arrange
        var txId = "tx-12345";
        var refundTxId = "refund-98765";
        var amount = 100.50m;
        var request = new RefundPaymentRequest(txId, amount);
        var ct = CancellationToken.None;

        var grpcResponse = new RefundResponse
        {
            IsSuccess = true,
            Message = "Refund processed successfully",
            RefundTransactionId = refundTxId
        };

        var call = new AsyncUnaryCall<RefundResponse>(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        _grpcClient.RefundAsync(
            Arg.Is<RefundRequest>(r => r.PaymobTransactionId == txId && r.Amount == 100.50),
            Arg.Any<Metadata>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns(call);

        // Act
        var result = await _client.RefundAsync(request, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Success.Should().BeTrue();
        result.Value.RefundTransactionId.Should().Be(refundTxId);

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Refund successful")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task RefundAsync_WhenServiceReturnsFailure_ReturnsFailedResultAndLogsWarning()
    {
        // Arrange
        var request = new RefundPaymentRequest("tx-123", 50.0m);
        var ct = CancellationToken.None;

        var grpcResponse = new RefundResponse
        {
            IsSuccess = false,
            Message = "Insufficient funds in merchant account",
            RefundTransactionId = ""
        };

        var call = new AsyncUnaryCall<RefundResponse>(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        _grpcClient.RefundAsync(
            Arg.Any<RefundRequest>(),
            Arg.Any<Metadata>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns(call);

        // Act
        var result = await _client.RefundAsync(request, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Insufficient funds in merchant account");

        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Refund failed")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task RefundAsync_WhenGrpcCallThrowsException_ReturnsFailedResultAndLogsWarning()
    {
        // Arrange
        var request = new RefundPaymentRequest("tx-123", 50.0m);
        var ct = CancellationToken.None;

        _grpcClient.RefundAsync(
            Arg.Any<RefundRequest>(),
            Arg.Any<Metadata>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Throws(new RpcException(new Status(StatusCode.Internal, "Internal Error")));

        // Act
        var result = await _client.RefundAsync(request, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Payment Service is unavailable, refund could not be processed");

        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("gRPC call to Payment Service failed")),
            Arg.Any<RpcException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
