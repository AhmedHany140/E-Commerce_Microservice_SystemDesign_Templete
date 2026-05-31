using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Infrastructure.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace ECommerce.ProductService.Infrastructure.Services
{
	public class ImplementedProductGrpcService : ProductGrpcService.ProductGrpcServiceBase
	{
		private readonly IProductRepository _productRepository;

		public ImplementedProductGrpcService(IProductRepository productRepository)
		{
			_productRepository = productRepository;
		}

		public override async Task<ProductResponse> GetProduct(
			GetProductRequest request,
			ServerCallContext context)
		{
			var product = await _productRepository.GetByIdAsync(Guid.Parse(request.Id));

			if (product is null)
			{
				throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
			}

			return new ProductResponse
			{
				Id = product.Id.ToString(),
				Name = product.Name,
				Description = product.Description ?? "",
				Price = (double)product.Price,
				StockQuantity = product.StockQuantity,
				CategoryId = product.CategoryId.ToString(),

				CreatedAt = Timestamp.FromDateTime(product.CreatedAt.ToUniversalTime()),

				UpdatedAt = product.UpdatedAt.HasValue
					? Timestamp.FromDateTime(product.UpdatedAt.Value.ToUniversalTime())
					: null
			};
		}
	}
}
