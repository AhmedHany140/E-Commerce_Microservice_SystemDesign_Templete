using ECommerce.OrderService.Infrastructure.Idempotency;
using ECommerce.OrderService.Api.Endpoints;
using ECommerce.OrderService.Api.Queries;
using ECommerce.OrderService.Application.Features.Orders.Cancel;
using ECommerce.OrderService.Application.Features.Orders.Payments;
using ECommerce.OrderService.Infrastructure;
using ECommerce.OrderService.Infrastructure.Services;
using ECommerce.Shared.Persistence;
using FluentValidation;
using JasperFx.Core;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Persistence;
using Wolverine.RabbitMQ;
using Wolverine.SqlServer;
using JasperFx.CodeGeneration.Model;
using HotChocolate.AspNetCore.CommandLine;
using MassTransit;
using ECommerce.Shared.Messages;

DotNetEnv.Env.Load();

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


// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<CancelOrderCommand>();

// Authorization
builder.Services.AddHttpContextAccessor();


builder.Host.UseWolverine(opts =>
{
	opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
	var connectionString = builder.Configuration.GetConnectionString("Constr");

	opts.Discovery.IncludeAssembly(typeof(CancelOrderCommand).Assembly);
	opts.Discovery.IncludeType<PaymentHandlers>();
	opts.ApplicationAssembly = typeof(OrderEndpoints).Assembly;

	if (builder.Configuration.GetValue("Wolverine:SqlPersistenceEnabled", false))
	{
		opts.PersistMessagesWithSqlServer(connectionString, "Service");
	}


	opts.UseEntityFrameworkCoreTransactions();

	opts.Policies.AutoApplyTransactions(IdempotencyStyle.Eager);

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

    opts.Policies.AddMiddleware(typeof(ECommerce.OrderService.Infrastructure.Idempotency.Context.WolverineIncomingIdempotencyMiddleware));
	var rabbitEnabled = builder.Configuration.GetValue("RabbitMq:Enabled", false);
	var rabbitUrl = builder.Configuration["RabbitMq:Url"];

	if (rabbitEnabled && Uri.TryCreate(rabbitUrl, UriKind.Absolute, out var rabbitUri))
	{
		opts.UseRabbitMq(rabbitUri).AutoProvision();
		opts.PublishMessage<RabbitMqEmailMessage>()
			.ToRabbitExchange(builder.Configuration["RabbitMq:EmailExchange"] ?? "auth-email-events");
	}

	opts.UseSystemTextJsonForSerialization();

	opts.ListenToRabbitQueue("payment-queue");
	opts.ListenToRabbitQueue("refund-queue");

});

builder.Services.AddWolverineHttp();

var rabbitEnabledForBus = builder.Configuration.GetValue("RabbitMq:Enabled", false);
if (rabbitEnabledForBus)
{
	builder.Services.AddMassTransit(x =>
	{
		x.UsingRabbitMq((context, cfg) =>
		{
			var rabbitUrl = builder.Configuration["RabbitMq:Url"]
				?? throw new InvalidOperationException("RabbitMq:Url is required when RabbitMq:Enabled is true.");
			cfg.Host(new Uri(rabbitUrl));
		});
	});
}
else
{
	builder.Services.AddMassTransit(x => x.UsingInMemory((context, cfg) => { }));
}

builder.Services.AddGrpc(options => 
{
    options.Interceptors.Add<ECommerce.OrderService.Infrastructure.Idempotency.Context.GrpcServerIdempotencyInterceptor>();
});

builder.Services
	.AddGraphQLServer()
	.AddAuthorization()
	.AddQueryType<Query>()
	.AddProjections()
	.AddFiltering()
	.AddSorting()
	.ModifyRequestOptions(opt =>
	{
		opt.IncludeExceptionDetails = true;
	});


builder.Services.AddEndpointsApiExplorer();




var app = builder.Build();


// Configure the HTTP request pipeline.

app.MapGraphQL("/graphql");

// Fusion uses the local HTTP GraphQL endpoint during development. Avoid a
// 307 redirect here because HttpClient may drop Authorization on redirects.
if (app.Environment.IsDevelopment())
{
	app.UseWhen(
		ctx => !ctx.Request.Path.StartsWithSegments("/graphql"),
		branch => branch.UseHttpsRedirection());
}
else
{
	app.UseHttpsRedirection();
}
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


app.UseMiddleware<ECommerce.OrderService.Infrastructure.Idempotency.Context.IdempotencyContextMiddleware>();
app.UseBusinessIdempotency();
app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});
app.MapGrpcService<ImplementationOrderService>();

if (args.Length > 0)
{
    app.RunWithGraphQLCommands(args);
}
else
{
    await app.StartAsync();
    if (builder.Configuration.GetValue("Wolverine:SqlPersistenceEnabled", false))
        await WolverineSchemaRepair.NormalizeReceivedAtAsync(builder.Configuration.GetConnectionString("Constr"), "Service");
    await app.WaitForShutdownAsync();
}



