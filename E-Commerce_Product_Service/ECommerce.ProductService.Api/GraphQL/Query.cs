using ECommerce.ProductService.Application.DTOs;
using ECommerce.ProductService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ProductService.Api.GraphQL;

public class Query
{
	
	[UsePaging(IncludeTotalCount =true)]
	[UseProjection]
	[UseFiltering]
	[UseSorting]
	public IQueryable<ProductDto> GetProducts([Service] ProductDbContext dbContext)
    {
		return dbContext.Products.Select(product => new ProductDto
		{
			Id = product.Id,
			Name = product.Name,
			Description = product.Description,
			Price = product.Price,
			StockQuantity = product.StockQuantity,
			CategoryId = product.CategoryId,
			Category = product.Category != null ? new CategoryDto
			{
				Id = product.Category.Id,
				Name = product.Category.Name,
				Description = product.Category.Description,
				CreatedAt = product.Category.CreatedAt,
				UpdatedAt = product.Category.UpdatedAt
			} : null,
			CreatedAt = product.CreatedAt,
			UpdatedAt = product.UpdatedAt
		});
	}


	public async Task<ProductDto?> GetProductById([Service] ProductDbContext dbContext, Guid id)
	{
		var product = await dbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
		if (product == null)
		{
			throw new GraphQLException("Product With this Id Not Found");
		}
		return new ProductDto
		{
			Id = product.Id,
			Name = product.Name,
			Description = product.Description,
			Price = product.Price,
			StockQuantity = product.StockQuantity,
			CategoryId = product.CategoryId,
			Category = product.Category != null ? new CategoryDto
			{
				Id = product.Category.Id,
				Name = product.Category.Name,
				Description = product.Category.Description,
				CreatedAt = product.Category.CreatedAt,
				UpdatedAt = product.Category.UpdatedAt
			} : null,
			CreatedAt = product.CreatedAt,
			UpdatedAt = product.UpdatedAt
		};
	}

	[UsePaging(IncludeTotalCount = true)]
	[UseProjection]
	[UseFiltering]
	[UseSorting]
	public IQueryable<CategoryDto> GetCategories([Service] ProductDbContext dbContext)
	{
		return dbContext.Categories.Select(category => new CategoryDto
		{
			Id = category.Id,
			Name = category.Name,
			Description = category.Description,
			CreatedAt = category.CreatedAt,
			UpdatedAt = category.UpdatedAt
		});
	}

	public async Task<CategoryDto?> GetCategoryById([Service] ProductDbContext dbContext, Guid id)
	{
		var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
		if (category == null)
		{
			throw new GraphQLException("Category With this Id Not Found");
		}
		return new CategoryDto
		{
			Id = category.Id,
			Name = category.Name,
			Description = category.Description,
			CreatedAt = category.CreatedAt,
			UpdatedAt = category.UpdatedAt
		};
	}

}
