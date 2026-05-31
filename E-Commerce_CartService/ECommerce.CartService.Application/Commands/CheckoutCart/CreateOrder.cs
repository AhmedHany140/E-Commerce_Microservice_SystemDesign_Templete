using ECommerce.CartService.Application.Common.Interfaces;
using FluentResults;
using FluentValidation;

namespace ECommerce.CartService.Application.Commands.CheckoutCart;

public record CreateOrderCommand(string UserId,string ShippingAddress);

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {

        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ShippingAddress).NotEmpty();
    }
}

public class CreateOrderHandler
{
    private readonly IOrderServiceClient _orderServiceClient;
	public CreateOrderHandler(IOrderServiceClient orderServiceClient)
	{
		_orderServiceClient = orderServiceClient;
	}

	public async Task<Result<CreateOrderDto>> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        //Create Order
        var CreateOrderResult = await _orderServiceClient
            .CreateOrderAsync(command.UserId, command.ShippingAddress, ct);

        return CreateOrderResult;
    }
}
