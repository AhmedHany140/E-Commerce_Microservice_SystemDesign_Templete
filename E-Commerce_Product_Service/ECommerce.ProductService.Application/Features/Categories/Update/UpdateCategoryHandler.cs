using ECommerce.ProductService.Application.Common.Interfaces;
using FluentResults;
using System.Threading;
using System.Threading.Tasks;
using Wolverine.Attributes;

namespace ECommerce.ProductService.Application.Features.Categories.Update;

public static class UpdateCategoryHandler
{
	[Transactional]
	public static async Task<Result> Handle(
        UpdateCategoryCommand command, ICategoryRepository repository, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (category == null)
            return Result.Fail("Category not found");

        var updateResult = category.Update(command.Name, command.Description);

        if (updateResult.IsFailed)
            return updateResult;

        repository.Update(category);

        return Result.Ok();
    }
}
