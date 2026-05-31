using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using FluentResults;
using Wolverine.Attributes;

namespace ECommerce.ProductService.Application.Features.Products.Delete;

public static class DeleteProductHandler
{
    [Transactional]
    public static async Task<Result> Handle(DeleteProductCommand command, IProductRepository _repository, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Result.Fail($"Product with Id {command.Id} not found.");

        _repository.Delete(product);

        return Result.Ok();
    }
}
