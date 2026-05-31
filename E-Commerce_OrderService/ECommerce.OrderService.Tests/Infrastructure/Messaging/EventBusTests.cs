using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.OrderService.Infrastructure.Messaging;
using MassTransit;
using NSubstitute;
using Xunit;

namespace ECommerce.OrderService.Tests.Infrastructure.Messaging;

public class EventBusTests
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventBus _eventBus;

    public EventBusTests()
    {
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _eventBus = new EventBus(_publishEndpoint);
    }

    [Fact]
    public async Task PublishAsync_WithValidMessage_PublishesThroughMassTransit()
    {
        // Arrange
        var testMessage = new TestMessage("Hello World");
        var ct = CancellationToken.None;

        // Act
        await _eventBus.PublishAsync(testMessage, ct);

        // Assert
        await _publishEndpoint.Received(1).Publish(testMessage, ct);
    }

    private record TestMessage(string Text);
}
