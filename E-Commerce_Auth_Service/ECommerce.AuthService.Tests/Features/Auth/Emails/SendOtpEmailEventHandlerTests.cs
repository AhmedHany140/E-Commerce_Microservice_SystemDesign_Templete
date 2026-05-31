using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Tests.Helpers;
using ECommerce.Shared.Messages;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Wolverine;

namespace ECommerce.AuthService.Tests.Features.Auth.Emails;

public class SendOtpEmailEventHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly IMessageBus _messageBus;

    public SendOtpEmailEventHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);
        _messageBus = Substitute.For<IMessageBus>();
    }

    [Fact]
    public async Task Handle_WithExistingUser_PublishesEmailMessage()
    {
        // Arrange
        var user = UserFaker.Generate();
        var otpEvent = new Application.Events.SendOtpEmailEvent(user.Email!, "Confirm Email", "123456");
        _userManager.FindByEmailAsync(user.Email!).Returns(user);

        // Act
        await Application.Features.Auth.Emails.SendOtpEmailEventHandler.Handle(otpEvent, _userManager, _messageBus);

        // Assert
        await _messageBus.Received(1).PublishAsync(Arg.Is<RabbitMqEmailMessage>(m =>
            m.To == user.Email &&
            m.Subject == "Confirm Email" &&
            m.Source == "AuthService" &&
            m.Template == "EmailOtpTemplate"));
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_DoesNotPublish()
    {
        // Arrange
        var otpEvent = new Application.Events.SendOtpEmailEvent("nobody@test.com", "Subject", "msg");
        _userManager.FindByEmailAsync("nobody@test.com").ReturnsNull();

        // Act
        await Application.Features.Auth.Emails.SendOtpEmailEventHandler.Handle(otpEvent, _userManager, _messageBus);

        // Assert
        await _messageBus.DidNotReceive().PublishAsync(Arg.Any<RabbitMqEmailMessage>());
    }
}
