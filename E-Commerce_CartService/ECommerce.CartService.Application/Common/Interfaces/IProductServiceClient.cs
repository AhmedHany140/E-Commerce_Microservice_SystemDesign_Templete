using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Application.Common.Models;
using FluentResults;

namespace ECommerce.CartService.Application.Common.Interfaces;

public interface IProductServiceClient
{
    Task<Result<ProductDto>> GetProductAsync(Guid productId,
        CancellationToken ct = default);
}


public interface IOrderServiceClient
{
	Task<Result<CreateOrderDto>> CreateOrderAsync(string UserId,
		string ShippingAddress,
		CancellationToken ct = default);
}

public interface IPaymentServiceClient
{
	Task<Result<string>> IntialPaymentAsync(InitiatePaymentRequest request,
		CancellationToken ct = default);
}

public record InitiatePaymentRequest(
	string OrderId,
	double Amount,
	PaymentMethod Method,
	string UserPhone,
	string UserEmail
);

public enum PaymentMethod
{
	Cash = 1,
	Card,
	Wallet,
	Kiosk
}

public record CreateOrderDto(string OrderId,
	double TotalPrice);

