using ECommerce.ProductService.Api.Requests;
using ECommerce.ProductService.Application.Features.Products.Create;
using ECommerce.ProductService.Application.Features.Products.Update;
using ECommerce.ProductService.Application.Features.Products.Delete;
using Riok.Mapperly.Abstractions;

namespace ECommerce.ProductService.Api.Mappers;

[Mapper]
public static partial class ProductMapper
{
    public static partial CreateProductCommand Map(CreateProductRequest request);
    public static partial UpdateProductCommand Map(UpdateProductRequest request);
    public static partial DeleteProductCommand Map(DeleteProductRequest request);
}
