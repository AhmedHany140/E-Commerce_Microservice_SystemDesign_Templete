using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Domain.Entities;
using FluentResults;
using Wolverine.Attributes;

namespace ECommerce.ProductService.Application.Features.Categories.Create;

public static class CreateCategoryHandler
{
    [Transactional]
    public static async Task<Result<Guid>> Handle(
        CreateCategoryCommand command, ICategoryRepository repository, CancellationToken cancellationToken)
    {
        var categoryResult = Category.Create(command.Name, command.Description);

        if (categoryResult.IsFailed)
            return categoryResult.ToResult();

        var category = categoryResult.Value;

        await repository.AddAsync(category, cancellationToken);

        return Result.Ok(category.Id);
    }
}
