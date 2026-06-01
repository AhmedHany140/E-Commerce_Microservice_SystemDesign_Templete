using ECommerce.OrderService.Api.Endpoints;
using ECommerce.OrderService.Api.Queries;
using ECommerce.OrderService.Application.Features.Orders.Cancel;
using ECommerce.OrderService.Infrastructure;
using ECommerce.OrderService.Infrastructure.Services;
using FluentValidation;
using JasperFx.Core;
using Microsoft.Data.SqlClient;
using Serilog;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Persistence;
using Wolverine.RabbitMQ;
using Wolverine.SqlServer;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);


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
	var connectionString = builder.Configuration.GetConnectionString("Constr");

	opts.Discovery.IncludeAssembly(typeof(CancelOrderCommand).Assembly);
	opts.ApplicationAssembly = typeof(OrderEndpoints).Assembly;

	opts.PersistMessagesWithSqlServer(connectionString, "Service");


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

	var rabbitUrl =
	   builder.Configuration["RabbitMqUrl:url"];

	opts.UseRabbitMq(new Uri(rabbitUrl))
		.AutoProvision();

	opts.UseSystemTextJsonForSerialization();

	opts.ListenToRabbitQueue("payment-queue");
	opts.ListenToRabbitQueue("refund-queue");

});

builder.Services.AddWolverineHttp();

builder.Services.AddGrpc();

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
builder.Services.AddSwaggerGen();




var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1"));


app.MapGraphQL("/graphql");

app.UseHttpsRedirection();
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


app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});
app.MapGrpcService<ImplementationOrderService>();

app.Run();
