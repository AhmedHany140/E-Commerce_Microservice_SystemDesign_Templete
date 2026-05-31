using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Domain.Entities;
using FluentResults;
using Wolverine.Attributes;

namespace ECommerce.ProductService.Application.Features.Products.Create;

public static class CreateProductHandler
{
    [Transactional]
    public static async Task<Result<Guid>> Handle(
        CreateProductCommand command, IProductRepository _repository, CancellationToken cancellationToken)
    {
        var productResult = Product.Create(command.Name, command.Description, command.Price, command.StockQuantity, command.CategoryId);

        if (productResult.IsFailed)
            return productResult.ToResult();

        var product = productResult.Value;

        await _repository.AddAsync(product, cancellationToken);

        return Result.Ok(product.Id);
    }
}
