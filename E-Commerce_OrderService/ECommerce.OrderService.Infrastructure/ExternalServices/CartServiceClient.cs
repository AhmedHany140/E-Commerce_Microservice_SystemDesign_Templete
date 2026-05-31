using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.OrderService.Application.Common.Interfaces;
using FluentResults;

namespace ECommerce.OrderService.Infrastructure.ExternalServices;

public class CartServiceClient : ICartServiceClient
{
    private readonly HttpClient _httpClient;

    public CartServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<CartDto>> GetCartAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/carts?userId={userId}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return Result.Fail("Cart not found.");

            if (!response.IsSuccessStatusCode)
                return Result.Fail($"Failed to fetch cart. Status: {response.StatusCode}");

            var cart = await response.Content.ReadFromJsonAsync<CartDto>(cancellationToken: ct);
            return cart != null ? Result.Ok(cart) : Result.Fail("Failed to deserialize cart.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error connecting to Cart Service: {ex.Message}");
        }
    }
}
