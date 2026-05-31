using ECommerce.ProductService.Api.Requests;
using ECommerce.ProductService.Application.Features.Categories.Create;
using ECommerce.ProductService.Application.Features.Categories.Update;
using ECommerce.ProductService.Application.Features.Categories.Delete;
using Riok.Mapperly.Abstractions;

namespace ECommerce.ProductService.Api.Mappers;

[Mapper]
public static partial class CategoryMapper
{
    public static partial CreateCategoryCommand Map(CreateCategoryRequest request);
    public static partial UpdateCategoryCommand Map(UpdateCategoryRequest request);
    public static partial DeleteCategoryCommand Map(DeleteCategoryRequest request);
}
