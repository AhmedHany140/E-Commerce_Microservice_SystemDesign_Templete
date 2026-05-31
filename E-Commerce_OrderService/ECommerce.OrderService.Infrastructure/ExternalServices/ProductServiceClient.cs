using ECommerce.OrderService.Application.Common.Interfaces;
using FluentResults;
using System.Net;
using System.Net.Http.Json;

namespace ECommerce.OrderService.Infrastructure.ExternalServices;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;

    public ProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<ProductDto>> GetProductAsync(Guid productId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/products/{productId}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return Result.Fail("Product not found.");

            if (!response.IsSuccessStatusCode)
                return Result.Fail($"Failed to fetch product. Status: {response.StatusCode}");

            var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: ct);
            return product != null ? Result.Ok(product) : Result.Fail("Failed to deserialize product.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error connecting to Product Service: {ex.Message}");
        }
    }
}
