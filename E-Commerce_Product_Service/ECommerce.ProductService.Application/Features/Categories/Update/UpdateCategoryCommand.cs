using System;

namespace ECommerce.ProductService.Application.Features.Categories.Update;

public record UpdateCategoryCommand(Guid Id, string Name, string Description);
