using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.ProductService.Infrastructure.Grpc;
using FluentResults;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using static ECommerce.OrderService.Application.Common.Interfaces.IProductServiceClient;

namespace ECommerce.OrderService.Infrastructure.Clients
{
	public class ProductServiceClient : IProductServiceClient
	{
		private readonly ProductGrpcService.ProductGrpcServiceClient _grpcClient;
		private readonly IMemoryCache _cache;
		private readonly ILogger<ProductServiceClient> _logger;

		public ProductServiceClient(
			ProductGrpcService.ProductGrpcServiceClient grpcClient,
			IMemoryCache cache,
			ILogger<ProductServiceClient> logger)
		{
			_grpcClient = grpcClient;
			_cache = cache;
			_logger = logger;
		}

		public async Task<Result<ProductDto>> GetProductAsync(Guid productId, CancellationToken ct = default)
		{
			var cacheKey = $"product_{productId}";

			// ? Cache
			if (_cache.TryGetValue(cacheKey, out ProductDto? cachedProduct) && cachedProduct != null)
			{
				return Result.Ok(cachedProduct);
			}

			try
			{
				// ? gRPC Call
				var response = await _grpcClient.GetProductAsync(
					new GetProductRequest { Id = productId.ToString() },
					cancellationToken: ct);

				// ? Mapping ? DTO
				var product = new ProductDto
				(
					Id: Guid.Parse(response.Id),
					Name: response.Name,
					Description: response.Description,
					Price: (decimal)response.Price,
					StockQuantity: response.StockQuantity
				);

				// ? Cache
				_cache.Set(cacheKey, product, TimeSpan.FromMinutes(5));

				return Result.Ok(product);
			}
			catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
			{
				return Result.Fail("Product not found via Product Service.");
			}
			catch (RpcException ex)
			{
				_logger.LogError(ex, "gRPC error while calling Product Service for ProductId: {ProductId}", productId);
				return Result.Fail("Product Service is unavailable.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while calling Product Service for ProductId: {ProductId}", productId);
				return Result.Fail("Unexpected error occurred.");
			}
		}
	}
}
