extern alias order;
using PaymentGrpc = order::ECommerce.PaymentService.Api.Grpc;
using Grpc.Core;
using Grpc.Net.Client;

static async Task Run(string name, Func<Task> action)
{
    try
    {
        await action();
        Console.WriteLine($"{name}: PASS");
    }
    catch (RpcException exception)
    {
        Console.WriteLine($"{name}: TRANSPORT_PASS status={exception.StatusCode} detail={exception.Status.Detail}");
    }
}

using var paymentChannel = GrpcChannel.ForAddress(
    "https://localhost:7053",
    new GrpcChannelOptions
    {
        HttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        }
    });
var paymentClient = new PaymentGrpc.PaymentGrpcService.PaymentGrpcServiceClient(paymentChannel);

await Run("payment-grpc", async () =>
{
    _ = await paymentClient.RefundAsync(new PaymentGrpc.RefundRequest
    {
        PaymobTransactionId = "grpc-smoke-invalid",
        Amount = 1
    });
});
