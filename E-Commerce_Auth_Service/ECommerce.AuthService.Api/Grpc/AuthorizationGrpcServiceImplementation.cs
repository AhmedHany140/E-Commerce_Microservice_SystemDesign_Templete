using Grpc.Core;
using Wolverine;
using Authorization; 
using ECommerce.AuthService.Application.Features.Auth.ValidateToken;

namespace ECommerce.AuthService.Api.Grpc;

public class AuthorizationGrpcServiceImplementation : AuthorizationGrpcService.AuthorizationGrpcServiceBase
{
    private readonly IMessageBus _messageBus;

    public AuthorizationGrpcServiceImplementation(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public override async Task<ValidateTokenResponse> ValidateToken(
        ValidateTokenRequest request, ServerCallContext context)
    {
        var response = await _messageBus.InvokeAsync
            <FluentResults.Result<ValidateTokenResponseDto>>
            (new ValidateTokenQuery(request.Token));

        var grpcResponse = new ValidateTokenResponse
        {
            IsValid = false
        };

        if (response.IsSuccess && response.Value != null)
        {
            grpcResponse.IsValid = response.Value.IsValid;
            if (response.Value.IsValid)
            {
                grpcResponse.UserId = response.Value.UserId;
                grpcResponse.Roles.AddRange(response.Value.Roles);
            }
        }

        return grpcResponse;
    }
}
