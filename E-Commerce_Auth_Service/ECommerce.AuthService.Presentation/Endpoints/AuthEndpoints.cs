using ECommerce.AuthService.Application.DTOs;
using ECommerce.AuthService.Application.Events;
using ECommerce.AuthService.Presentation.Mapping;
using ECommerce.AuthService.Presentation.Requests;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Wolverine;
using Wolverine.Http;

namespace ECommerce.AuthService.Presentation.Endpoints
{
	public static class AuthEndpoints
	{
		
		[WolverinePost("/api/auth/register")]
		public static async Task<IResult> Handle(RegisterRequest request,
			IMessageBus _bus)
		{
			var command = request.ToCommand();

			var result = await _bus.InvokeAsync<Result>(command);


			return result.IsFailed? 
				Results.BadRequest(result.Errors):
				Results.Ok();

		}
		[WolverinePost("/api/auth/login")]

		public static async Task<IResult> Handle(LoginRequest request,
			IMessageBus _bus)
		{
			var command = request.ToCommand();
			var result = await _bus.InvokeAsync<Result<LoginResponse>>(command);
			return result.IsSuccess
				? Results.Ok(result.Value)
				: Results.BadRequest(result.Errors);
		}


		[WolverinePost("/api/auth/change-password")]
		public static async Task<IResult> Handle(ChangePasswordRequest request,
			IMessageBus _bus)
		{
			var command = request.ToCommand();
			var result = await _bus.InvokeAsync<Result>(command);
			return result.IsSuccess
				? Results.Ok()
				: Results.BadRequest(result.Errors);
		}


		[WolverinePost("/api/auth/refresh-token")]
		public static async Task<IResult> Handle(RefreshTokenRequest request,
			IMessageBus _bus)
		{
			var command = request.ToCommand();
			var result = await _bus.InvokeAsync<Result<LoginResponse>>(command);
			return result.IsSuccess
				? Results.Ok(result.Value)
				: Results.BadRequest(result.Errors);
		}
		[WolverinePost("/api/auth/confirm-email")]
		public static async Task<IResult> Handle(ConfirmEmailRequest request, IMessageBus _bus)
		{
			var command = request.ToCommand();
			var result = await _bus.InvokeAsync<Result>(command);
			return result.IsSuccess
				? Results.Ok()
				: Results.BadRequest(result.Errors);
		}
		[WolverinePost("/api/auth/forgot-password")]
		public static async Task<IResult> Handle(ForgotPasswordRequest request, IMessageBus _bus)
		{
			var command = request.ToCommand();
			var result = await _bus.InvokeAsync<Result>(command);
			return result.IsSuccess
				? Results.Ok()
				: Results.BadRequest(result.Errors);
		}

		[WolverinePost("/api/auth/reset-password")]
		public static async Task<IResult> Handle(ResetPasswordRequest request,
			IMessageBus _bus)
		{
			var command = request.ToCommand();
			var result = await _bus.InvokeAsync<Result>(command);
			return result.IsSuccess
				? Results.Ok()
				: Results.BadRequest(result.Errors);
		}
	}
}
