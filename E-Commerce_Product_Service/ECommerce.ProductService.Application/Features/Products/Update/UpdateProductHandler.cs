using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using FluentResults;
using Wolverine.Attributes;

namespace ECommerce.ProductService.Application.Features.Products.Update;

public static class UpdateProductHandler
{

    [Transactional]
 
    public static async Task<Result> Handle(UpdateProductCommand command,
		IProductRepository _repository,
		CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Result.Fail($"Product with Id {command.Id} not found.");

        var updateResult = product.Update(command.Name, command.Description, command.Price, command.StockQuantity, command.CategoryId);

        if (updateResult.IsFailed)
            return updateResult;

        _repository.Update(product);

        return Result.Ok();
    }
}
