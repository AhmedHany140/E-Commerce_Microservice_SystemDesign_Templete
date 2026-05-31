using ECommerce.ProductService.Application.Common.Interfaces;
using FluentResults;
using System.Threading;
using System.Threading.Tasks;
using Wolverine.Attributes;

namespace ECommerce.ProductService.Application.Features.Categories.Delete;

public static class DeleteCategoryHandler
{
	[Transactional]
	public static async Task<Result> Handle(
        DeleteCategoryCommand command, ICategoryRepository repository, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (category == null)
            return Result.Fail($"Category with Id {command.Id} not found.");

        repository.Delete(category);

        return Result.Ok();
    }
}
