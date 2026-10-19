using DotNetEnv;
using PaymentService.Api.Dtos;
using PaymentService.Api.Endpoints;
using ECommerce.OrderService.Application.Features.Orders.Payments;
using PaymentService.Api.Interfaces;
using PaymentService.Api.Services;
using PaymentService.Api.Idempotency;
using PaymentService.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Wolverine;
using Wolverine.RabbitMQ;
using Microsoft.AspNetCore.Server.Kestrel.Core;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
    options.ConfigureEndpointDefaults(endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));

Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File(
		"logs/log-.txt",
		rollingInterval: RollingInterval.Day)
	.CreateLogger();

builder.Host.UseSerilog();


builder.Host.UseWolverine(opts =>
{


	//Auto -apply idempotency to all handlers that do not have DB Transactions. This means that even handlers that do not interact with the database will still benefit from idempotency, preventing duplicate processing of messages in scenarios where transactions are not used.
	opts.Policies.AutoApplyIdempotencyOnNonTransactionalHandlers();

	opts.Policies.AddMiddleware(typeof(PaymentService.Api.Idempotency.Context.WolverineIncomingIdempotencyMiddleware));

	var rabbitEnabled = builder.Configuration.GetValue("RabbitMq:Enabled", false);
	var rabbitUrl = builder.Configuration["RabbitMq:Url"];

	if (rabbitEnabled && Uri.TryCreate(rabbitUrl, UriKind.Absolute, out var rabbitUri))
	{
		opts.UseRabbitMq(rabbitUri).AutoProvision();
	}

	opts.UseSystemTextJsonForSerialization();

	opts.PublishMessage<PaymentSucceededEvent>()
		.ToRabbitQueue("payment-queue");

	opts.PublishMessage<PaymentFailedEvent>()
	   .ToRabbitQueue("payment-queue");

	opts.PublishMessage<RefundResult>()
	.ToRabbitQueue("refund-queue");

	opts.UseSystemTextJsonForSerialization();

});




builder.Services.AddOpenApi();
builder.Services.AddGrpc(options => 
{
    options.Interceptors.Add<PaymentService.Api.Idempotency.Context.GrpcServerIdempotencyInterceptor>();
});

builder.Services.AddSingleton<PaymentService.Api.Idempotency.Context.IIdempotencyContextAccessor, PaymentService.Api.Idempotency.Context.IdempotencyContextAccessor>();
builder.Services.AddSingleton<PaymentService.Api.Idempotency.IIdempotencyKeyProvider, PaymentService.Api.Idempotency.IdempotencyKeyProvider>();

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<PaymobSettings>(
	builder.Configuration.GetSection("Paymob"));

builder.Services.AddHttpClient<IPaymobService, PaymobService>();
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.")));
builder.Services.AddScoped<IIdempotencyStore, IdempotencyStore>();
builder.Services.AddScoped<IBusinessIdempotencyProcessor, BusinessIdempotencyProcessor>();
builder.Services.AddHostedService<IdempotencyCleanupService>();

var app = builder.Build();


app.UseSerilogRequestLogging(options =>
{
	options.MessageTemplate =
		"Handled {RequestMethod} {RequestPath} => {StatusCode} in {Elapsed:0.0000} ms";

	options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
	{
		diagnosticContext.Set("RemoteIP",
			httpContext.Connection.RemoteIpAddress);

		diagnosticContext.Set("UserAgent",
			httpContext.Request.Headers.UserAgent.ToString());
	};
});


if (app.Environment.IsDevelopment())
	app.MapOpenApi();

PaymentEndpoints.Map(app);

app.UseWhen(
	ctx => !(ctx.Request.ContentType?.StartsWith("application/grpc") ?? false)
		&& !ctx.Request.Path.StartsWithSegments("/api/payment/webhook"),
	b => b.UseHttpsRedirection()
);

app.MapGrpcService<PaymentGrpcServiceImpl>();

app.Run();

