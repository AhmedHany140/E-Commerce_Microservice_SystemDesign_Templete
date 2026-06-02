using ECommerce.AuthService.Infrastructure.Idempotency;
using DotNetEnv;
using ECommerce.AuthService.Api.Grpc;
using ECommerce.AuthService.Api.Middleware;
using ECommerce.AuthService.Application.Features.Auth.Login;
using ECommerce.AuthService.Infrastructure;
using ECommerce.AuthService.Presentation;
using ECommerce.Shared.Messages;
using FluentValidation;
using Humanizer;
using Microsoft.Data.SqlClient;
using Serilog;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Http;
using Wolverine.Persistence;
using Wolverine.RabbitMQ;
using Wolverine.SqlServer;
using ECommerce.AuthService.Infrastructure.Idempotency.Context;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ── Logging ──────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File(
		"logs/log-.txt",
		rollingInterval: RollingInterval.Day)
	.CreateLogger();

builder.Host.UseSerilog();


// ── Infrastructure (Persistence, Identity, JWT, MassTransit, Services) ──
builder.Services.AddInfrastructure(builder.Configuration);

// ── Presentation / Validation ────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<PresentationMarker>();

// ── Wolverine ────────────────────────────────────────────────────────
builder.Host.UseWolverine(opts =>
{
	var connectionString = builder.Configuration.GetConnectionString("Constr");

	opts.ApplicationAssembly = typeof(LoginCommand).Assembly;
	opts.Discovery.IncludeAssembly(typeof(PresentationMarker).Assembly);


	opts.PersistMessagesWithSqlServer(connectionString, "CartService");


	opts.UseEntityFrameworkCoreTransactions();

	//Auto -apply idempotency to all handlers that are Has DB Transactions ( those that use Entity Framework Core transactions). This means that any handler that interacts with the database will automatically have idempotency applied, ensuring that duplicate messages are not processed multiple times.
	opts.Policies.AutoApplyTransactions(IdempotencyStyle.Eager);

	//Auto -apply idempotency to all handlers that do not have DB Transactions. This means that even handlers that do not interact with the database will still benefit from idempotency, preventing duplicate processing of messages in scenarios where transactions are not used.
	opts.Policies.AutoApplyIdempotencyOnNonTransactionalHandlers();

	opts.Durability.KeepAfterMessageHandling = 24.Hours();

	opts.Durability.Mode = DurabilityMode.Solo;

	opts.Policies
	.OnException<SqlException>()
	.RetryWithCooldown(
		1.Seconds(),
		5.Seconds(),
		15.Seconds());

	opts.Policies
		.OnException<ValidationException>()
		.Discard();

	opts.Policies
		.OnAnyException()
		.MoveToErrorQueue();

	opts.Policies.AutoApplyTransactions();

    opts.Policies.AddMiddleware(typeof(ECommerce.AuthService.Infrastructure.Idempotency.Context.WolverineIncomingIdempotencyMiddleware));
var rabbitUrl = builder.Configuration["RabbitMq:Url"];

	opts.UseRabbitMq(new Uri(rabbitUrl))
		.AutoProvision();

	opts.UseSystemTextJsonForSerialization();

	opts.PublishMessage<RabbitMqEmailMessage>()
		.ToRabbitQueue("email-queue");

	opts.Policies.AutoApplyTransactions();
	opts.Policies.UseDurableLocalQueues();
});

// ── Swagger / HTTP / gRPC ────────────────────────────────────────────
builder.Services.AddSwaggerGen();
builder.Services.AddWolverineHttp();
builder.Services.AddGrpc(options => 
{
    options.Interceptors.Add<ECommerce.AuthService.Infrastructure.Idempotency.Context.GrpcServerIdempotencyInterceptor>();
});

var app = builder.Build();

// ── Middleware pipeline ──────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1"));

app.UseAuthentication();
app.UseAuthorization();

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



app.UseMiddleware<TokenBlacklistMiddleware>();

app.UseMiddleware<IdempotencyContextMiddleware>();
app.UseBusinessIdempotency();
app.MapWolverineEndpoints();
app.MapGrpcService<AuthorizationGrpcServiceImplementation>();

app.Run();


